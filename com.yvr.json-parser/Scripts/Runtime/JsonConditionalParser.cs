using System;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using YVR.Utilities;

namespace YVR.JsonParser
{
    public class JsonConditionalParser
    {
        private string m_Json = null;

        private List<KeyValuePair<Tuple<string, Type>, Func<object, bool>>> m_ToMetConditionsPairList
            = new List<KeyValuePair<Tuple<string, Type>, Func<object, bool>>>();

        private Dictionary<string, Action<object>> m_Key2OnConditionFailedDict = new Dictionary<string, Action<object>>();

        public JsonConditionalParser() { }

        public JsonConditionalParser(string json) { m_Json = json; }

        public void SetJson(string json) { m_Json = json; }

        public void AddCondition<T>(string key, Func<T, bool> toMetCondition)
        {
            Tuple<string, Type> keyAValueTypeTuple = new Tuple<string, Type>(key, typeof(T));

            KeyValuePair<Tuple<string, Type>, Func<object, bool>> keyConditionPair
                = new KeyValuePair<Tuple<string, Type>, Func<object, bool>>(keyAValueTypeTuple, Convert(toMetCondition));

            m_ToMetConditionsPairList.Add(keyConditionPair);
        }

        public void AddConditionFailCallback<T>(string key, Action<T> onConditionFailed) { m_Key2OnConditionFailedDict.Add(key, Convert(onConditionFailed)); }

        public bool Deserialize<T>(string objKey, out T obj)
        {
            obj = default;
            T result = default;
            bool succeed = Parse(objKey, obj, (jToken, targetObj) => result = JsonParserMgr.DeserializeObject<T>(jToken.ToString()));
            obj = result;

            return succeed;
        }

        public bool Populate<T>(string objKey, T obj)
        {
            return Parse(objKey, obj, (jToken, targetObj) => JsonParserMgr.PopulateObject(jToken.ToString(), targetObj));
        }

        private Func<object, bool> Convert<T>(Func<T, bool> condition)
        {
            if (condition == null) return null;
            return o =>
            {
                if (!(o is T arg)) return false;
                return condition(arg);
            };
        }

        private Action<object> Convert<T>(Action<T> onConditionFail)
        {
            if (onConditionFail == null) return null;
            return o => onConditionFail((T) o);
        }

        private bool Parse<T>(string objKey, T obj, Action<JToken, T> parseOperation)
        {
            JObject jObj = JObject.Parse(m_Json);
            List<KeyValuePair<string, object>> unMatchedList = new List<KeyValuePair<string, object>>();
            m_ToMetConditionsPairList.ForEach(pair =>
            {
                string key = pair.Key.Item1;
                Type targetValType = pair.Key.Item2;
                object actualVal = jObj.SelectToken(key)?.ToObject(targetValType);

                bool met = pair.Value.Invoke(actualVal);

                if (!met) unMatchedList.Add(new KeyValuePair<string, object>(key, actualVal));
            });

            bool succeed = unMatchedList.Count == 0;

            if (succeed)
            {
                try
                {
                    JToken jToken = string.IsNullOrEmpty(objKey) ? jObj.Root : jObj.SelectToken(objKey);
                    parseOperation(jToken, obj);
                } catch (Exception e)
                {
                    this.Warn($"Exception {e} when parsing json");
                    succeed = false;
                }
            }

            if (!succeed)
            {
                // Call registered conditionalFailed callback
                unMatchedList.ForEach(pair =>
                {
                    bool hasOnFailedCallback = m_Key2OnConditionFailedDict.TryGetValue(pair.Key, out Action<object> onFail);
                    if (hasOnFailedCallback)
                        onFail.Invoke(pair.Value);
                });
            }

            return succeed;
        }
    }
}