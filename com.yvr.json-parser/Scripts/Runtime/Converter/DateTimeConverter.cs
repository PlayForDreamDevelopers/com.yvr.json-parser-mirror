using System;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using UnityEngine;
using YVR.Utilities;

public class DateTimeConverter : JsonConverter
{
    private static Regex s_DateTimeRegex = new Regex(@"\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}");
    public override bool CanConvert(Type objectType) => objectType.Equals(typeof(DateTime));

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        try
        {
            string value = reader.Value.ToString();
            ////example: 1648118400000 > DateTime(03/24/2022 18:40:00)
            if (long.TryParse(value, out long result))
            {
                DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));

                long sec = result * 10000;
                DateTime targetTime = startTime.Add(new TimeSpan(sec));

                return targetTime;
            }
            else if (s_DateTimeRegex.IsMatch(value)) //example: 2022-03-28 20:48:29 > DateTime(03/28/2022 20:48:29)
                return Convert.ToDateTime(value);
        } catch (System.Exception e)
        {
            this.Error($"parse date time error:{reader?.Value},{e}");
            return DateTime.MinValue;
        }

        return DateTime.Now;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        //example: DateTime(03/24/2022 18:40:00) > 1648118400000
        DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
        long unixTime = (long) Math.Round(((DateTime) value - startTime).TotalMilliseconds,
                                          MidpointRounding.AwayFromZero);
        writer.WriteRawValue(unixTime.ToString());
    }
}