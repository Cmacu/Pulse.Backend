using Pulse.Core.Entities;
using Pulse.Core.Services;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace Pulse.Core.Models
{
    public class PlayerSettingsModel
    {
        public bool EmailNotifications { get; set; }

        public PlayerSettingsModel()
        {
            EmailNotifications = true;
        }
    }
}
