using Newtonsoft.Json;

namespace VSTSSprintMetrics.VSTSClient.VSTSAPIReponses
{
    internal class WorkItemByIterationResponse
    {
        [JsonProperty("workItemRelations")]
        public WorkItemRelation[] Relations { get; set; }
    }

    internal class WorkItemRelation
    {
        [JsonProperty("source")]
        public WorkItemIDUrl Source { get; set; }

        [JsonProperty("target")]
        public WorkItemIDUrl Target { get; set; }
    }

    internal class WorkItemIDUrl
    {
        [JsonProperty("id")]
        public int ID { get; set; }
    }
}
