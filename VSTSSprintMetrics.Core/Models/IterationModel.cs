using System;

namespace VSTSSprintMetrics.Core.Models
{
    public class IterationModel
    {
        public enum IterationTimescale
        {
            Past,
            Future,
            Current
        }

        public string ID { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public IterationTimescale Timescale { get; set; }
    }
}
