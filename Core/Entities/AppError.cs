using System;
using System.ComponentModel.DataAnnotations;

namespace Pulse.Core.Entities {
    public class AppError {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string Message { get; set; }

        [MaxLength(10000)]
        public string Details { get; set; }
    }
}