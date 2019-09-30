using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using VSTSSprintMetrics.Core.Interfaces.Services;
using VSTSSprintMetrics.Core.Models;
using VSTSSprintMetrics.WebUI.ViewModels;

namespace VSTSSprintMetrics.WebUI.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class MetricsController : ControllerBase
    {
        private readonly IMetricsService mMetricsService;

        public MetricsController(IMetricsService metricsService)
        {
            mMetricsService = metricsService;
        }

        [HttpGet]
        [Route("sprintmetrics")]
        public async Task<IEnumerable<WorkItemMetricsViewModel>> GetSprintMetrics(string projectID, string teamID, string iterationID)
        {
            var metrics = await mMetricsService.GetSprintMetricsAsync(projectID, teamID, iterationID);
            return metrics.Select(x => new WorkItemMetricsViewModel(x));
        }

        [HttpGet]
        [Route("crosssprintmetrics")]
        public async Task<CrossSprintMetricsModel> CrossSprintMetrics(string projectID, string iterationID, int workItemID)
        {
            return await mMetricsService.GetCrossSprintMetricsAsync(projectID, iterationID, workItemID);
        }
    }
}