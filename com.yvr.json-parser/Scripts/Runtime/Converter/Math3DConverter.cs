using Newtonsoft.Json;
using System;
using System.Text;
using System.Linq;
using UnityEngine.Internal;

namespace YVR.JsonParser
{
    [ExcludeFromDocs]
    public class Math3DConverter : JsonConverter
    {
        private Type m_Type;
        public Math3DConverter(Type type) { m_Type = type; }

        public override bool CanConvert(Type objectType) => objectType == m_Type;

        public override bool CanRead => false;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            throw new NotImplementedException(
                "Unnecessary because CanRead is false. The type will skip the converter.");
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteRawValue(GetRawJson(value));
        }

        protected string GetRawJson<T>(T jsonObject)
        {
            var fields = jsonObject.GetType().GetFields();
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            fields.Where(field => field.IsPublic && !field.IsLiteral).ToList().ForEach(field =>
                sb.Append($"\"{field.Name}\":{field.GetValue(jsonObject)},"));
            sb.Remove(sb.Length - 1, 1); // Remove last ","
            sb.Append("}");
            return sb.ToString();
        }
    }
}