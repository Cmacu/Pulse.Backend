using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace Pulse.Core.Notifications {
    public class EmailLog {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string To { get; set; }
        public string Subject { get; set; }

        [MaxLength(10000)]
        public string Body { get; set; }

    }

}