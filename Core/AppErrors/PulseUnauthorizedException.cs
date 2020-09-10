using System;

namespace Pulse.Core.AppErrors {
    public class PulseUnauthorizedException : Exception {
        public PulseUnauthorizedException() : base() {}
        public PulseUnauthorizedException(string message) : base(message) {}
    }
}