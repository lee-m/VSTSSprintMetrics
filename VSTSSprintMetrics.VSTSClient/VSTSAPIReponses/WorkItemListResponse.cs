using Newtonsoft.Json;

namespace VSTSSprintMetrics.VSTSClient.VSTSAPIReponses
{
    internal class WorkItemListResponse
    {
        [JsonProperty("value")]
        public WorkItemResponse[] WorkItems { get; set; }
    }
}
