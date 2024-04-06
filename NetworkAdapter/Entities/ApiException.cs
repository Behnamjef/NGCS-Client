using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkAdapter.Entities
{
    public class ApiException : Exception
    {
        public int StatusCode { get; set; }
        public string Name { get; set; }

        public ApiException(string name, string message)
            : this(400, name, message) { }

        public ApiException(int statusCode, string name, string message)
            : base(message)
        {
            StatusCode = statusCode;
            Name = name;
        }

        public ApiError GetApiError() => new ApiError { code = StatusCode, name = Name, message = Message };

        public static ApiException UnknownError => new ApiException(-1, "unknown-error", "Something went wrong");
        public static ApiException NoResponse => new ApiException(-2, "no-response", "No response from server");
        public static ApiException Unauthorized => new ApiException(401, "unauthorized", "Unauthorized");
        public static ApiException CreateLocalError(string message)
        {
            return new ApiException(100, "local-error", message);
        }

        public override string ToString()
        {
            return Message;
        }
    }

    public struct ApiError
    {
        public int code;
        public string name;
        public string message;

        public ApiException GetException() => new ApiException(code, name, message);

        public string ToDetailedString()
        {
            return string.Format("{0} - {1} - {2}", code, name, message);
        }
    }
}
