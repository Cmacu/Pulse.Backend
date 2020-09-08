using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace Pulse.Core.Entities
{
    public class EmailLog
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }

    }

}