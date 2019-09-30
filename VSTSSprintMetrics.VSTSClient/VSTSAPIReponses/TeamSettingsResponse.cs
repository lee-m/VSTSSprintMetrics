using Newtonsoft.Json;

namespace VSTSSprintMetrics.VSTSClient.VSTSAPIReponses
{
    internal class TeamSettingsResponse
    {
        [JsonProperty("workingDays")]
        public string[] WorkingDays { get; set; }
    }
}
