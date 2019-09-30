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
    public class VSTSIterationsRepository : IVSTSIterationsRepository
    {
        private readonly VSTSApiSettings mSettings;
        private readonly VSTSClient mClient;

        private const string IterationsListAPIUrl = "https://dev.azure.com/{organization}/{project}/{team}/_apis/work/teamsettings/iterations?api-version=4.1";

        public VSTSIterationsRepository(IOptions<VSTSApiSettings> settings, HttpClient httpClient)
        {
            mSettings = settings.Value;
            mClient = new VSTSClient(mSettings, httpClient);
        }

        public async Task<IEnumerable<IterationModel>> GetIterationsAsync(string projectID, string teamID)
        {
            var url = IterationsListAPIUrl.Replace("{organization}", mSettings.AccountName)
                                          .Replace("{project}", projectID)
                                          .Replace("{team}", teamID);
            var response = await mClient.ExecuteActionAsync<IterationsResponse>(url);

            return response.Iterations.Select(x => ConvertIterationResponseToModel(x));
        }

        public async Task<IterationModel> GetIteration(string projectID, string teamID, string iterationID)
        {
            var url = IterationsListAPIUrl.Replace("{organization}", mSettings.AccountName)
                                          .Replace("{project}", projectID)
                                          .Replace("{team}", teamID);
            var response = await mClient.ExecuteActionAsync<IterationsResponse.IterationResponse>(url);

            return ConvertIterationResponseToModel(response);
        }

        private IterationModel ConvertIterationResponseToModel(IterationsResponse.IterationResponse response)
        {
            return new IterationModel()
            {
                EndDate = response.Attributes.FinishDate,
                ID = response.ID,
                Name = response.Name,
                Path = response.Path,
                StartDate = response.Attributes.StartDate,
                Timescale = ParseIterationTimescale(response.Attributes.Timescale)
            };
        }
        private IterationModel.IterationTimescale ParseIterationTimescale(string timescale)
        {
            if (timescale == "past")
                return IterationModel.IterationTimescale.Past;
            else if (timescale == "current")
                return IterationModel.IterationTimescale.Current;
            else
                return IterationModel.IterationTimescale.Future;
        }
    }
}
