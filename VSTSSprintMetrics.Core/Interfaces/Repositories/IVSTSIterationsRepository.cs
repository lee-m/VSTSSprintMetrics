using System.Collections.Generic;
using System.Threading.Tasks;

using VSTSSprintMetrics.Core.Models;

namespace VSTSSprintMetrics.Core.Interfaces.Repositories
{
    public interface IVSTSIterationsRepository
    {
        Task<IEnumerable<IterationModel>> GetIterationsAsync(string projectID, string teamID);
        Task<IterationModel> GetIteration(string projectID, string teamID, string iterationID);
    }
}
