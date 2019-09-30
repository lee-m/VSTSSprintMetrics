using System;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace VSTSSprintMetrics.VSTSClient.VSTSAPIReponses
{
    internal class TeamCapacityResponse
    {
        [JsonProperty("values")]
        public IEnumerable<TeamMemberCapacityResponse> Capacities { get; set; }
    }

    internal class TeamMemberCapacityResponse
    {
        [JsonProperty("teamMember")]
        public TeamMemberResponse TeamMember { get; set; }

        [JsonProperty("activities")]
        public IEnumerable<ActivityResponse> Activities { get; set; }

        [JsonProperty("daysOff")]
        public List<DaysOffResponse> DaysOff { get; set; }
    }

    internal class TeamMemberResponse
    {
        [JsonProperty("id")]
        public string ID { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("uniqueName")]
        public string UniqueName { get; set; }
    }

    internal class ActivityResponse
    {
        [JsonProperty("capacityPerDay")]
        public int CapacityPerDay { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    internal class DaysOffResponse
    {
        [JsonProperty("start")]
        public DateTime Start { get; set; }

        [JsonProperty("end")]
        public DateTime End { get; set; }
    }

}
