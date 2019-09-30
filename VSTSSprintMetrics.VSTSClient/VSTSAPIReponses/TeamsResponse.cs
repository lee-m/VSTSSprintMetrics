using Newtonsoft.Json;

namespace VSTSSprintMetrics.VSTSClient.VSTSAPIReponses
{
    internal class TeamsResponse
    {
        [JsonProperty("value")]
        public TeamResponse[] Teams { get; set; }

        internal class TeamResponse
        {
            [JsonProperty("id")]
            public string ID { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("projectName")]
            public string ProjectName { get; set; }

            [JsonProperty("projectId")]
            public string ProjectID { get; set; }
        }
    }


}
