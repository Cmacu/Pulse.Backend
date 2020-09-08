using System;

namespace Pulse.Core.Entities
{
    public class AppError
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string Message { get; set; }
        public string Details { get; set; }
    }
}