using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace BulkDataLoader
{
    public static class ConfigurationExtensions
    {
        public static T Get<T>(this Dictionary<string, object> properties, string name, T defaultValue)
        {
            if (properties == null || !properties.ContainsKey(name))
            {
                return defaultValue;
            }

            var value = properties[name].ToString();
            return JsonConvert.DeserializeObject<T>(value);
        }

        public static TOut Get<TIn, TOut>(this Dictionary<string, object> properties, string name, TOut defaultValue, Func<TIn, TOut> formatFunc)
        {
            if (properties == null || !properties.ContainsKey(name))
            {
                return defaultValue;
            }

            var value = properties[name].ToString();
            return formatFunc(JsonConvert.DeserializeObject<TIn>(value));
        }


        public static bool TryGet<T>(this Dictionary<string, object> properties, string name, out T output)
        {
            if (properties != null && properties.TryGetValue(name, out var value))
            {
                output = JsonConvert.DeserializeObject<T>(value.ToString());
                return true;
            }

            output = default;
            return false;
        }
    }
}
