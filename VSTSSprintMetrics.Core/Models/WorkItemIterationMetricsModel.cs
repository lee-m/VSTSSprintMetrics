namespace VSTSSprintMetrics.Core.Models
{
    public class WorkItemIterationMetricsModel
    {
        public string Iteration { get; set; }
        public float EstimatedTime { get; set; }
        public float TotalTime { get; set; }
        public float TimeTesting { get; set; }
        public float TimeDeveloping { get; set; }
        public float TimeBugFixing { get; set; }
        public int NumberOfBugs { get; set; }
        public int NumberOfOpenBugs { get; set; }
    }
}
