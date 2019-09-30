using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using VSTSSprintMetrics.Core.Interfaces.Repositories;
using VSTSSprintMetrics.Core.Models;

namespace VSTSSprintMetrics.WebUI.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class VSTSController : ControllerBase
    {
        private readonly IVSTSTeamsRepository mTeamsRepository;
        private readonly IVSTSIterationsRepository mIterationsRepository;

        public VSTSController(IVSTSTeamsRepository teamsRepository, 
                              IVSTSIterationsRepository iterationsRepository)
        {
            mTeamsRepository = teamsRepository;
            mIterationsRepository = iterationsRepository;
        }

        [HttpGet]
        [Route("teams")]
        public async Task<IEnumerable<TeamModel>> Teams()
        {
            return await mTeamsRepository.GetTeamsAsync();
        }

        [HttpGet]
        [Route("iterations")]
        public async Task<IEnumerable<IterationModel>> Iterations(string teamID, string projectID)
        {
            var iterations = await mIterationsRepository.GetIterationsAsync(projectID, teamID);

            return iterations.OrderByDescending(x => x.StartDate)
                             .Where(x => x.Timescale != IterationModel.IterationTimescale.Future)
                             .Select(x => new IterationModel()
                             {
                                 Name = x.Name + (x.Timescale == IterationModel.IterationTimescale.Current ? " (Current)" : " (Past)"),
                                 ID = x.ID
                             });
        }
    }
}
