using VSTSSprintMetrics.Core.Models;

namespace VSTSSprintMetrics.WebUI.ViewModels
{
    public class WorkItemMetricsViewModel
    {
        public WorkItemMetricsViewModel()
        { }

        public WorkItemMetricsViewModel(WorkItemMetricsModel model)
        {
            WorkItemID = model.WorkItemID;
            WorkItemTitle = model.WorkItemTitle;
            FormattedDevelopment = model.SumCompletedDev == 0 ? "Not Started" : $"{model.SumCompletedDev} / {model.SumOriginalDev} ({model.DevAccuracy.ToString("P0")})";
            FormattedTesting = model.SumCompletedTesting == 0 ? "Not Started" : $"{model.SumCompletedTesting} / {model.SumOriginalTesting} ({model.TestingAccuracy.ToString("P0")})";
            FormattedOverall = $"{model.SumCompleted} / {model.SumOriginal} ({model.OverallAccuracy.ToString("P0")})";
            Closed = model.Closed;
            DevelopmentAccuracy = model.DevAccuracy * 100;
            TestingAccuracy = model.TestingAccuracy * 100;
            OverallAccuracy = model.OverallAccuracy * 100;
        }

        public int WorkItemID { get; set; }
        public string WorkItemTitle { get; set; }
        public string FormattedDevelopment { get; set; }
        public string FormattedTesting { get; set; }
        public string FormattedOverall { get; set; }
        public float DevelopmentAccuracy { get; set; }
        public float TestingAccuracy { get; set; }
        public float OverallAccuracy { get; set; }
        public bool Closed { get; set; }
    }
}
