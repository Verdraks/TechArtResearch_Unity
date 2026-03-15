using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace MVsToolkit.Favorites.Editor
{
    public class ColorConverter : JsonConverter<Color>
    {
        public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("r");
            writer.WriteValue(value.r);
            writer.WritePropertyName("g");
            writer.WriteValue(value.g);
            writer.WritePropertyName("b");
            writer.WriteValue(value.b);
            writer.WritePropertyName("a");
            writer.WriteValue(value.a);
            writer.WriteEndObject();
        }

        public override Color ReadJson(JsonReader reader, Type objectType, Color value, bool hasExistingValue,
            JsonSerializer serializer)
        {
            JObject jsonObject = JObject.Load(reader);
            
            if (jsonObject["r"] != null)
                value.r = jsonObject["r"].Value<float>();
            if (jsonObject["g"] != null)
                value.g = jsonObject["g"].Value<float>();
            if (jsonObject["b"] != null)
                value.b = jsonObject["b"].Value<float>();
            if (jsonObject["a"] != null)
                value.a = jsonObject["a"].Value<float>();
            
            return value;
        }
    }
}