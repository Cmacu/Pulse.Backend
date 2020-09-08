using System;

namespace Pulse.Exceptions
{
    public class MyUnauthorizedException : Exception
    {
        public MyUnauthorizedException() : base() {}
        public MyUnauthorizedException(string message) : base(message) {}
    }
}