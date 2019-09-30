using System.Collections.Generic;
using System.Threading.Tasks;

using VSTSSprintMetrics.Core.Models;

namespace VSTSSprintMetrics.Core.Interfaces.Services
{
    public interface IMetricsService
    {
        Task<IEnumerable<WorkItemMetricsModel>> GetSprintMetricsAsync(string projectID, string teamID, string iterationID);
        Task<CrossSprintMetricsModel> GetCrossSprintMetricsAsync(string projectID, string iterationID, int workItemID);
    }
}
