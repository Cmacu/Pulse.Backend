using System.Net;
using Pulse.Core.AppErrors;

namespace Pulse.Core.Authorization {
    public class AuthException : AppException {
        public AuthException() : base(HttpStatusCode.Unauthorized) {}
        public AuthException(string message) : base(HttpStatusCode.Unauthorized, message) {}
    }
}