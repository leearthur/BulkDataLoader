namespace BulkDataLoader
{
    public static class DataGeneratorExtensions
    {
        public static string ReplaceFirst(this string value, string oldValue, string newValue)
        {
            var position = value.IndexOf(oldValue);
            if (position < 0)
            {
                return value;
            }
            var result = value.Substring(0, position) + newValue + value.Substring(position + oldValue.Length);
            return result;
        }
    }
}
