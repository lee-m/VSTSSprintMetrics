using System.Collections.Generic;
using System.Threading.Tasks;

using VSTSSprintMetrics.Core.Models;

namespace VSTSSprintMetrics.Core.Interfaces.Repositories
{
    public interface IVSTSWorkItemsRepository
    {
        Task<IEnumerable<WorkItemModel>> GetWorkItemsForIterationAsync(string projectID, string teamID, string iterationID);
        Task<WorkItemModel> GetExpandedWorkItemAsync(string projectID, int workItemID);
    }
}
