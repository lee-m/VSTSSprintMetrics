using System;
using System.Collections.Generic;

namespace VSTSSprintMetrics.Core.Models
{
    public class TeamSettingsModel
    {
        public IEnumerable<DayOfWeek> WorkingDays { get; set; }
    }
}
