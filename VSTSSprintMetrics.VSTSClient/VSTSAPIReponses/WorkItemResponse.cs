using System;

using Newtonsoft.Json;

namespace VSTSSprintMetrics.VSTSClient.VSTSAPIReponses
{
    internal class WorkItemResponse
    {
        [JsonProperty("id")]
        public int ID { get; set; }

        [JsonProperty("fields")]
        public WorkItemFields Fields { get; set; }

        [JsonProperty("relations")]
        public Relation[] Relations { get; set; }
    }

    internal class WorkItemFields
    {
        [JsonProperty("System.IterationPath")]
        public string IterationPath { get; set; }

        [JsonProperty("System.WorkItemType")]
        public string WorkItemType { get; set; }

        [JsonProperty("System.State")]
        public string State { get; set; }

        [JsonProperty("System.Title")]
        public string Title { get; set; }

        [JsonProperty("Microsoft.VSTS.Scheduling.RemainingWork")]
        public float? RemainingWork { get; set; }

        [JsonProperty("Microsoft.VSTS.Scheduling.OriginalEstimate")]
        public float? OriginalEstimate { get; set; }

        [JsonProperty("Microsoft.VSTS.Scheduling.CompletedWork")]
        public float? CompletedWork { get; set; }

        [JsonProperty("Microsoft.VSTS.Common.Activity")]
        public string Activity { get; set; }
    }

    public class Relation
    {
        [JsonProperty("rel")]
        public string Relationship { get; set; }

        [JsonProperty("url")]
        public string URL { get; set; }
    }
}
