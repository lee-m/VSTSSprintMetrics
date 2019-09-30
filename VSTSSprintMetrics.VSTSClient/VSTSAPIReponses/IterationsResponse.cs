using System;

using Newtonsoft.Json;

namespace VSTSSprintMetrics.VSTSClient.VSTSAPIReponses
{
    internal class IterationsResponse
    {
        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("value")]
        public IterationResponse[] Iterations { get; set; }

        public class IterationResponse
        {
            [JsonProperty("id")]
            public string ID { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("path")]
            public string Path { get; set; }

            [JsonProperty("attributes")]
            public IterationAttributes Attributes { get; set; }

        }

        public class IterationAttributes
        {
            [JsonProperty("startDate")]
            public DateTime? StartDate { get; set; }

            [JsonProperty("finishDate")]
            public DateTime? FinishDate { get; set; }

            [JsonProperty("timeFrame")]
            public string Timescale { get; set; }
        }
    }
}
