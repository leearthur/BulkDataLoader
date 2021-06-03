using System;

namespace BulkDataLoader.Tests
{
    public static class TestExtensions
    {
        public static string StripQuotes(this string value)
        {
            return value[1..^1];
        }

        public static DateTime RemoveMilliseconds(this DateTime dateTime)
        {
            if (dateTime == DateTime.MinValue || dateTime == DateTime.MaxValue)
            {
                return dateTime;
            }
            var result = new DateTime(dateTime.Ticks - (dateTime.Ticks % TimeSpan.FromSeconds(1).Ticks), dateTime.Kind);

            return result;
        }
    }
}
