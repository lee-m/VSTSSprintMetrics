using System.Collections.Generic;
using System.Threading.Tasks;

using VSTSSprintMetrics.Core.Models;

namespace VSTSSprintMetrics.Core.Interfaces
{
    public interface IDataCache
    {
        Task<IEnumerable<WorkItemMetricsModel>> GetSprintMetricsAsync(string teamID, string iterationID);
        Task<CrossSprintMetricsModel> GetCrossSprintMetrics(string iterationID, string workItemID);

        Task InsertSprintMetricsAsync(string teamID, string iterationID, IEnumerable<WorkItemMetricsModel> metrics);

        Task BulkInsertCrossSprintMetrics(string iterationD, IEnumerable<CrossSprintMetricsModel> metrics);
    }
}
