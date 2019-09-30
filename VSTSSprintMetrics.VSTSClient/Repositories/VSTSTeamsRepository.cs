using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

using VSTSSprintMetrics.Core.Interfaces.Repositories;
using VSTSSprintMetrics.Core.Models;
using VSTSSprintMetrics.VSTSClient.VSTSAPIReponses;

namespace VSTSSprintMetrics.VSTSClient.Repositories
{
    public class VSTSTeamsRepository : IVSTSTeamsRepository
    {
        private readonly VSTSApiSettings mSettings;
        private readonly VSTSClient mClient;

        private const string TeamsAPIUrl = "https://dev.azure.com/{organization}/_apis/teams?api-version=4.1-preview.2";
        private const string TeamSettingsAPIUrl = "https://dev.azure.com/{organization}/{project}/{team}/_apis/work/teamsettings?api-version=4.1";
        private const string TeamCapacityAPIUrl = "https://dev.azure.com/{organization}/{project}/{team}/_apis/work/teamsettings/iterations/{iterationId}/capacities?api-version=4.1";

        public VSTSTeamsRepository(IOptions<VSTSApiSettings> settings, HttpClient httpClient)
        {
            mSettings = settings.Value;
            mClient = new VSTSClient(mSettings, httpClient);
        }

        public async Task<IEnumerable<TeamModel>> GetTeamsAsync()
        {
            var response = await mClient.ExecuteActionAsync<TeamsResponse>(TeamsAPIUrl.Replace("{organization}", mSettings.AccountName));

            return response.Teams.Select(x => new TeamModel()
            {
                FormattedName = $"{x.ProjectName}\\{x.Name}",
                ID = x.ID,
                Name = x.Name,
                ProjectID = x.ProjectID
            }).OrderBy(x => x.FormattedName);
        }

        public async Task<TeamSettingsModel> GetTeamSettingsAsync(string projectID, string teamID)
        {
            var url = TeamSettingsAPIUrl.Replace("{organization}", mSettings.AccountName)
                                        .Replace("{project}", projectID)
                                        .Replace("{team}", teamID);
            var response = await mClient.ExecuteActionAsync<TeamSettingsResponse>(url);

            return new TeamSettingsModel()
            {
                WorkingDays = response.WorkingDays.Select(x => (DayOfWeek)Enum.Parse(typeof(DayOfWeek), x, true))
            };
        }

        public async Task<int> GetTeamCapacityForIteration(string projectID, string teamID, string iterationID)
        {
            var url = TeamCapacityAPIUrl.Replace("{organization}", mSettings.AccountName)
                                        .Replace("{project}", projectID)
                                        .Replace("{team}", teamID)
                                        .Replace("{iterationId}", iterationID);
            var response = await mClient.ExecuteActionAsync<TeamCapacityResponse>(url);
            return 0;
        }
    }
}
