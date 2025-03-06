using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using YVR.Utilities;

namespace YVR.JsonParser
{
    public class JsonParserMgr
    {
        private static JsonConverter[] s_Converters;
        private static JsonConverter[] converters => s_Converters ??= convertersDict.Values.ToArray();

        private static Dictionary<Type, JsonConverter> s_CustomizeConverters;

        private static Dictionary<Type, JsonConverter> convertersDict
        {
            get
            {
                return s_CustomizeConverters ??= new Dictionary<Type, JsonConverter>()
                {
                    {typeof(Vector2), new Math3DConverter(typeof(Vector2))},
                    {typeof(Vector3), new Math3DConverter(typeof(Vector3))},
                    {typeof(Vector4), new Math3DConverter(typeof(Vector4))},
                    {typeof(Quaternion), new Math3DConverter(typeof(Quaternion))},
                    {typeof(Matrix4x4), new Math3DConverter(typeof(Matrix4x4))},
                };
            }
        }

        /// <summary>
        ///  Customize converters for special data types (common data in Unity);
        /// </summary>
        /// <param name="type"></param>
        /// <param name="converter"></param>
        public static void AddConverter(Type type, JsonConverter converter)
        {
            convertersDict.SafeAdd(type, converter);
            s_Converters = convertersDict.Values.ToArray();
        }

        public static bool IsValidJson(string jsonStr)
        {
            if (string.IsNullOrWhiteSpace(jsonStr)) return false;

            jsonStr = jsonStr.Trim();

            bool wrapValid = (jsonStr.StartsWith("{") && jsonStr.EndsWith("}")) ||
                             (jsonStr.StartsWith("[") && jsonStr.EndsWith("]"));
            if (!wrapValid) return false;

            try
            {
                JToken.Parse(jsonStr);
                return true;
            } catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Deserialize the specified wrapper of the Json content to a list of the specified data type.
        /// </summary>
        /// <typeparam name="T">The Specified data type</typeparam>
        /// <param name="json">The data to be Converted</param>
        /// <param name="wrapper">The specified wrapper</param>
        /// <returns>The list of target data</returns>
        public static List<T> GetList<T>(string json, string wrapper = null)
        {
            if (string.IsNullOrEmpty(wrapper))
                return DeserializeObject<List<T>>(json);

            List<T> result = new List<T>();
            List<JToken> contents = GetJTokenChildren(json, wrapper);

            foreach (JToken node in contents)
            {
                T nodeObj = node.ToObject<T>();
                result.Add(nodeObj);
            }

            return result;
        }

        /// <summary>
        /// Deserialize the Json content to a dictionary of the specified data type
        /// </summary>
        /// <typeparam name="T">The Specified data type</typeparam>
        /// <param name="json">The data to be Converted</param>
        /// <returns>The Dictionary of target data</returns>
        public static Dictionary<string, T> GetDictionary<T>(string json)
        {
            return DeserializeObject<Dictionary<string, T>>(json);
        }

        /// <summary>
        /// Deserialize the specified wrapper of the Json content to a dictionary of the specified data type
        /// </summary>
        /// <typeparam name="T">The Specified data type</typeparam>
        /// <param name="json">The data to be Converted</param>
        /// <param name="wrapper">The specified wrapper</param>
        /// <returns>The Dictionary of target data</returns>
        public static Dictionary<string, T> GetDictionary<T>(string json, string wrapper)
        {
            if (string.IsNullOrEmpty(wrapper))
                return GetDictionary<T>(json);

            Dictionary<string, T> searchResults = new Dictionary<string, T>();
            List<JToken> contents = GetJTokenChildren(json, wrapper);

            contents?.ForEach(jToken =>
            {
                //JToken.ToObject is a helper method that uses JsonSerializer internally
                string newJson = $"{{{jToken}}}";
                try
                {
                    var t = DeserializeObject<Dictionary<string, T>>(newJson);
                    foreach (var item in t)
                    {
                        if (item.Value != null)
                        {
                            searchResults.Add(item.Key, item.Value);
                        }
                    }
                } catch (Exception)
                {
                    // ignored
                }
            });
            return searchResults;
        }

        /// <summary>
        /// Deserialize Json content to specified data type.
        /// </summary>
        /// <typeparam name="T">The Specified data type</typeparam>
        /// <param name="json">The data to be Converted</param>
        /// <returns>Target data</returns>
        public static T DeserializeObject<T>(string json)
        {
            // If target type is string, there is no need to do further conversion.
            if (typeof(T) == typeof(string)) return (T) Convert.ChangeType(json, typeof(T));
            if (typeof(T) == typeof(bool))
            {
                bool.TryParse(json, out bool ret);
                return (T) Convert.ChangeType(ret, typeof(T));
            }


            return JsonConvert.DeserializeObject<T>(json, converters);
        }

        public static object DeserializeObject(string json, Type type)
        {
            return JsonConvert.DeserializeObject(json, type, converters);
        }

        /// <summary>
        /// Deserializes the JSON string to an object of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize to.</typeparam>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="settings">The JsonSerializerSettings used for deserialization.</param>
        /// <returns>The deserialized object.</returns>
        public static object DeserializeObject<T>(string json, JsonSerializerSettings settings)
        {
            return JsonConvert.DeserializeObject<T>(json, settings);
        }

        public static bool TryDeserializeObject<T>(string json, string targetName, out T obj)
        {
            obj = default;
            T result = default;
            bool succeed = TryParseJson(json, targetName, obj,
                                        (jToken, _) => result = DeserializeObject<T>(jToken.ToString()));
            obj = result;

            return succeed;
        }

        public static void PopulateObject<T>(string json, T obj, bool reuseIfPossible = true)
        {
            var settings = new JsonSerializerSettings
            {
                ObjectCreationHandling = reuseIfPossible ? ObjectCreationHandling.Auto : ObjectCreationHandling.Replace,
                Converters = s_Converters
            };
            JsonConvert.PopulateObject(json, obj, settings);
        }

        public static bool TryPopulateObject<T>(string json, string targetName, T obj)
        {
            return TryParseJson(json, targetName, obj,
                                (jToken, targetObj) => PopulateObject(jToken.ToString(), targetObj));
        }

        /// <summary>
        /// Serialize data to Json.
        /// </summary>
        /// <param name="value">The data to be serialized</param>
        /// <returns>The json content</returns>
        public static string SerializeObject(object value) { return JsonConvert.SerializeObject(value, converters); }

        /// <summary>
        /// Serialize data to Json with specified settings.
        /// </summary>
        /// <param name="value"> The data to be serialized </param>
        /// <param name="settings"> The specified settings </param>
        /// <returns> Serialized json content </returns>
        public static string SerializeObject(object value, JsonSerializerSettings settings)
        {
            return JsonConvert.SerializeObject(value, settings);
        }

        /// <summary>
        /// Get the target JToken of the wrapper
        /// </summary>
        /// <param name="json">The data to be Converted</param>
        /// <param name="wrapper">The specified wrapper</param>
        /// <returns>The target JToken</returns>
        public static JToken GetTargetJToken(string json, string wrapper)
        {
            JObject jObject = JObject.Parse(json);
            return jObject.SelectToken(wrapper);
        }

        /// <summary>
        /// Get the children JToken  of the wrapper
        /// </summary>
        /// <param name="json">The data to be Converted</param>
        /// <param name="wrapper">The specified wrapper</param>
        /// <returns>The children JToken</returns>
        public static List<JToken> GetJTokenChildren(string json, string wrapper)
        {
            JToken jToken = GetTargetJToken(json, wrapper);
            return jToken?.Children().ToList();
        }

        /// <summary>
        /// Get the value of JToken
        /// </summary>
        /// <param name="jToken">The JToken to be Converted</param>
        /// <typeparam name="T">The Specified data type</typeparam>
        /// <returns>The target data</returns>
        public static T GetJTokenValue<T>(in JToken jToken) { return jToken.ToObject<T>(); }


        private static bool TryParseJson<T>(string json, string targetName, T obj,
                                            Action<JToken, T> parseOperation)
        {
            bool getSucceed = true;
            try
            {
                JToken jToken = JObject.Parse(json).SelectToken($"['{targetName}']");
                parseOperation(jToken, obj);
            } catch (Exception)
            {
                getSucceed = false;
            }

            return getSucceed;
        }
    }
}