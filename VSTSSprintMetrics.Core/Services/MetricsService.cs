using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using VSTSSprintMetrics.Core.Interfaces;
using VSTSSprintMetrics.Core.Interfaces.Repositories;
using VSTSSprintMetrics.Core.Interfaces.Services;
using VSTSSprintMetrics.Core.Models;

namespace VSTSSprintMetrics.Core.Services
{
    public class MetricsService : IMetricsService
    {
        private IVSTSWorkItemsRepository mWorkItemsRepository;
        private IDataCache mCache;

        public MetricsService(IVSTSWorkItemsRepository workItemsRepository, IDataCache cache)
        {
            mWorkItemsRepository = workItemsRepository;
            mCache = cache;
        }

        public async Task<IEnumerable<WorkItemMetricsModel>> GetSprintMetricsAsync(string projectID, string teamID, string iterationID)
        {
            var cachedData = await mCache.GetSprintMetricsAsync(teamID, iterationID);

            if (cachedData != null)
                return cachedData;

            var workItems = await mWorkItemsRepository.GetWorkItemsForIterationAsync(projectID, teamID, iterationID);
            return CalculateSprintMetrics(workItems);
        }

        public async Task<CrossSprintMetricsModel> GetCrossSprintMetricsAsync(string projectID, string iterationID, int workItemID)
        {
            var cachedData = await mCache.GetCrossSprintMetrics(iterationID, workItemID.ToString());

            if (cachedData != null)
                return cachedData;

            var parentWorkItem = await mWorkItemsRepository.GetExpandedWorkItemAsync(projectID, workItemID);
            return CalculateCrossSprintMetricsForWorkItem(parentWorkItem);
        }

        /// <summary>
        /// Calculates metrics for a set of work items in a single sprint.
        /// </summary>
        /// <param name="workItems"></param>
        /// <returns></returns>
        private IEnumerable<WorkItemMetricsModel> CalculateSprintMetrics(IEnumerable<WorkItemModel> workItems)
        {
            return workItems.AsParallel().Where(x => x.IsUserStory).Select(topLevelWorkItem =>
            {
                var metrics = new WorkItemMetricsModel
                {
                    WorkItemID = topLevelWorkItem.ID,
                    WorkItemTitle = topLevelWorkItem.Title,
                    Closed = topLevelWorkItem.Closed
                };

                foreach (var childItem in topLevelWorkItem.ChildItems)
                {
                    if (!childItem.Closed)
                        continue;

                    switch (childItem.Activity)
                    {
                        case "Development":
                            metrics.SumCompletedDev += childItem.CompletedWork.GetValueOrDefault();
                            metrics.SumOriginalDev += childItem.OriginalEstimate.GetValueOrDefault();
                            break;

                        case "Testing":
                            metrics.SumCompletedTesting += childItem.CompletedWork.GetValueOrDefault();
                            metrics.SumOriginalTesting += childItem.OriginalEstimate.GetValueOrDefault();
                            break;
                    }
                }

                metrics.SumOriginal = metrics.SumOriginalDev + metrics.SumOriginalTesting;
                metrics.SumCompleted = metrics.SumCompletedDev + metrics.SumCompletedTesting;

                if (metrics.SumCompletedDev != 0)
                    metrics.DevAccuracy = (float)Math.Round(metrics.SumOriginalDev / metrics.SumCompletedDev, 2);

                if (metrics.SumCompletedTesting != 0)
                    metrics.TestingAccuracy = (float)Math.Round(metrics.SumOriginalTesting / metrics.SumCompletedTesting, 2);

                if(metrics.SumCompleted != 0)
                    metrics.OverallAccuracy = (float)Math.Round(metrics.SumOriginal / metrics.SumCompleted, 2);

                return metrics;
            });
        }

        /// <summary>
        /// Calculates cross-sprint metrics for a top-level work item (i.e. User Story) and all its children.
        /// </summary>
        /// <param name="parent">Top level work item.</param>
        /// <returns></returns>
        public CrossSprintMetricsModel CalculateCrossSprintMetricsForWorkItem(WorkItemModel parent)
        {
            var crossSprintMetrics = new CrossSprintMetricsModel
            {
                IterationMetrics = parent.ChildItems
                                         .GroupBy(x => x.IterationPath)
                                         .OrderBy(x => x.Key)
                                         .Select(x => CalculateWorkItemIterationMetrics(x))
                                         .Where(x => x.TotalTime > 0),
                WorkItemTitle = parent.Title,
                WorkItemID = parent.ID,
            };
            crossSprintMetrics.NumberOfIterations = crossSprintMetrics.IterationMetrics.Count();

            foreach (var iterationMetric in crossSprintMetrics.IterationMetrics)
            {
                crossSprintMetrics.NumberOfBugs += iterationMetric.NumberOfBugs;
                crossSprintMetrics.NumberOfOpenBugs += iterationMetric.NumberOfOpenBugs;
                crossSprintMetrics.TimeBugFixing += iterationMetric.TimeBugFixing;
                crossSprintMetrics.TimeDeveloping += iterationMetric.TimeDeveloping;
                crossSprintMetrics.TimeTesting += iterationMetric.TimeTesting;
                crossSprintMetrics.TotalTime += iterationMetric.TotalTime;
                crossSprintMetrics.TotalEstimatedTime += iterationMetric.EstimatedTime;
            }

            crossSprintMetrics.BugFixingPercentageOfDev = crossSprintMetrics.TimeBugFixing / crossSprintMetrics.TimeDeveloping;

            return crossSprintMetrics;
        }

        /// <summary>
        /// Calculates the metrics for a set of child work items that belong to the same iteration.
        /// </summary>
        /// <param name="iterationWorkItems"></param>
        /// <returns></returns>
        private WorkItemIterationMetricsModel CalculateWorkItemIterationMetrics(IGrouping<string, WorkItemModel> iterationWorkItems)
        {
            var metrics = new WorkItemIterationMetricsModel
            {
                Iteration = iterationWorkItems.Key.Split('\\').Last()
            };

            foreach (var workItem in iterationWorkItems)
            {
                metrics.TotalTime += workItem.CompletedWork.GetValueOrDefault();
                metrics.EstimatedTime += workItem.OriginalEstimate.GetValueOrDefault();

                if (workItem.IsBug)
                {
                    if (!workItem.IsClosed)
                        ++metrics.NumberOfOpenBugs;

                    ++metrics.NumberOfBugs;
                    metrics.TimeBugFixing += workItem.CompletedWork.GetValueOrDefault();

                }
                else
                {
                    if (workItem.IsTestingActivity)
                        metrics.TimeTesting += workItem.CompletedWork.GetValueOrDefault();
                    else
                        metrics.TimeDeveloping += workItem.CompletedWork.GetValueOrDefault();
                }
            }

            return metrics;
        }
    }
}
