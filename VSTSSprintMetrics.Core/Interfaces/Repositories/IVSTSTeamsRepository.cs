using System.Collections.Generic;
using System.Threading.Tasks;

using VSTSSprintMetrics.Core.Models;

namespace VSTSSprintMetrics.Core.Interfaces.Repositories
{
    public interface IVSTSTeamsRepository
    {
        Task<IEnumerable<TeamModel>> GetTeamsAsync();
        Task<TeamSettingsModel> GetTeamSettingsAsync(string projectID, string teamID);
    }
}
