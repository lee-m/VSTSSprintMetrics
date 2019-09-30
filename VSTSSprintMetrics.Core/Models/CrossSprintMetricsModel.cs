using System.Collections.Generic;

namespace VSTSSprintMetrics.Core.Models
{
    public class CrossSprintMetricsModel
    {
        public int WorkItemID { get; set; }
        public string WorkItemTitle { get; set; }
        public float TotalEstimatedTime { get; set; }
        public float TotalTime { get; set; }
        public float TimeTesting { get; set; }
        public float TimeDeveloping { get; set; }
        public float TimeBugFixing { get; set; }
        public float BugFixingPercentageOfDev { get; set; }
        public int NumberOfBugs { get; set; }
        public int NumberOfOpenBugs { get; set; }
        public int NumberOfIterations { get; set; }
        public IEnumerable<WorkItemIterationMetricsModel> IterationMetrics { get; set; }
    }
}
