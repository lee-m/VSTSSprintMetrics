using System;
using System.Collections.Generic;

namespace VSTSSprintMetrics.Core.Models
{
    public class WorkItemModel
    {
        public int ID { get; set; }
        public string IterationPath { get; set; }
        public string WorkItemType { get; set; }
        public string State { get; set; }
        public string Title { get; set; }
        public float? RemainingWork { get; set; }
        public float? OriginalEstimate { get; set; }
        public float? CompletedWork { get; set; }
        public string Activity { get; set; }

        public List<WorkItemModel> ChildItems { get; set; }

        public bool Closed => State == "Closed";
        public bool IsBug => WorkItemType == "Bug";
        public bool IsUserStory => WorkItemType == "User Story";
        public bool IsClosed => State == "Closed";
        public bool IsDevelopmentActivity => Activity == "Development";
        public bool IsTestingActivity => Activity == "Testing";
    }
}
