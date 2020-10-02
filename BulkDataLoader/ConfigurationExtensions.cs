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

            //return (T)Convert.ChangeType(value, typeof(T));
        }
    }
}
