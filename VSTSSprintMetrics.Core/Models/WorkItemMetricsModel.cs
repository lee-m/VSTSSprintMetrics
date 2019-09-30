namespace VSTSSprintMetrics.Core.Models
{
    public class WorkItemMetricsModel
    {
        public int WorkItemID { get; set; }
        public string WorkItemTitle { get; set; }
        public bool Closed { get; internal set; }

        public float SumOriginal { get; set; }
        public float SumCompleted { get; set; }
        public float OverallAccuracy { get; set; }

        public float SumOriginalDev { get; set; }
        public float SumCompletedDev { get; set; }
        public float DevAccuracy { get; set; }

        public float SumOriginalTesting { get; set; }
        public float SumCompletedTesting { get; set; }
        public float TestingAccuracy { get; set; }
    }
}
