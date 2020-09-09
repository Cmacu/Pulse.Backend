using System;

namespace Pulse.Exceptions {
    public class PulseUnauthorizedException : Exception {
        public PulseUnauthorizedException() : base() {}
        public PulseUnauthorizedException(string message) : base(message) {}
    }
}