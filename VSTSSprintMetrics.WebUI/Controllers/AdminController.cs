using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using VSTSSprintMetrics.Core.Interfaces;
using VSTSSprintMetrics.Core.Interfaces.Repositories;
using VSTSSprintMetrics.Core.Interfaces.Services;
using VSTSSprintMetrics.Core.Models;

namespace VSTSSprintMetrics.WebUI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IVSTSTeamsRepository mTeamsRepository;
        private readonly IVSTSIterationsRepository mIterationsRepository;
        private readonly IVSTSWorkItemsRepository mWorkItemsRepository;

        private readonly IMetricsService mMetricsCalculator;
        private readonly IDataCache mCache;

        public AdminController(IVSTSTeamsRepository teamsRepo,
                               IVSTSIterationsRepository iterationsRepo,
                               IVSTSWorkItemsRepository workItemsRepo,
                               IMetricsService metricsCalculator,
                               IDataCache cache)
        {
            mTeamsRepository = teamsRepo;
            mIterationsRepository = iterationsRepo;
            mWorkItemsRepository = workItemsRepo;

            mMetricsCalculator = metricsCalculator;
            mCache = cache;
        }

        public async Task SeedCaches()
        {
            var teams = await mTeamsRepository.GetTeamsAsync();

            foreach(var team in teams)
            {
                var iterations = await mIterationsRepository.GetIterationsAsync(team.ProjectID, team.ID);

                foreach(var iteration in iterations)
                {
                    if (iteration.Timescale != IterationModel.IterationTimescale.Past)
                        continue;

                    var workItems = await mWorkItemsRepository.GetWorkItemsForIterationAsync(team.ProjectID, team.ID, iteration.ID);

                    if (!workItems.Any())
                        continue;

                    var metrics = await mMetricsCalculator.GetSprintMetricsAsync(team.ProjectID, team.ID, iteration.ID);
                    await mCache.InsertSprintMetricsAsync(team.ID, iteration.ID, metrics);

                    //Cache cross-sprint metrics for each completed user story in the iteration
                    var crossSprintMetrics = new List<CrossSprintMetricsModel>();
                    var closedUserStories = workItems.Where(x => x.WorkItemType == "User Story" && x.Closed);

                    foreach(var userStoryItem in closedUserStories)
                        crossSprintMetrics.Add(await mMetricsCalculator.GetCrossSprintMetricsAsync(team.ProjectID, iteration.ID, userStoryItem.ID));

                    await mCache.BulkInsertCrossSprintMetrics(iteration.ID, crossSprintMetrics);
                }
            }
        }
    }
}