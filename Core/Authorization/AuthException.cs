using System;

namespace Pulse.Core.Authorization {
    public class AuthException : Exception {
        public AuthException() : base() {}
        public AuthException(string message) : base(message) {}
    }
}