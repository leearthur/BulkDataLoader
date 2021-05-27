using System;

namespace BulkDataLoader.Exceptions
{
    public class RequestValidationException : Exception
    {
        public RequestValidationException(string message) : base(message)
        {
        }
    }
}
