using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

using VSTSSprintMetrics.Core.Interfaces.Repositories;
using VSTSSprintMetrics.Core.Models;
using VSTSSprintMetrics.VSTSClient.VSTSAPIReponses;

namespace VSTSSprintMetrics.VSTSClient.Repositories
{
    public class VSTSWorkItemsRepository : IVSTSWorkItemsRepository
    {
        private readonly VSTSApiSettings mSettings;
        private readonly VSTSClient mClient;

        private const string WorkItemsByIterationAPIUrl = "https://dev.azure.com/{organization}/{project}/{team}/_apis/work/teamsettings/iterations/{iterationId}/workitems?api-version=4.1-preview.1";
        private const string WorkItemsListAPIUrl = "https://dev.azure.com/{organization}/{project}/_apis/wit/workitems?ids={ids}&api-version=4.1";
        private const string ExpandedRelationsWorkItemDetailsAPIUrl = "https://dev.azure.com/{organization}/{project}/_apis/wit/workitems/{id}?api-version=4.1&$expand=relations";

        public VSTSWorkItemsRepository(IOptions<VSTSApiSettings> settings, HttpClient httpClient)
        {
            mSettings = settings.Value;
            mClient = new VSTSClient(mSettings, httpClient);
        }

        public async Task<IEnumerable<WorkItemModel>> GetWorkItemsForIterationAsync(string projectID, string teamID, string iterationID)
        {
            if (string.IsNullOrEmpty(projectID))
                throw new ArgumentNullException(nameof(projectID));

            if (string.IsNullOrEmpty(teamID))
                throw new ArgumentNullException(nameof(teamID));

            if (string.IsNullOrEmpty(iterationID))
                throw new ArgumentNullException(nameof(iterationID));

            var url = WorkItemsByIterationAPIUrl.Replace("{organization}", mSettings.AccountName)
                                                .Replace("{project}", projectID)
                                                .Replace("{team}", teamID)
                                                .Replace("{iterationId}", iterationID);
            var response = await mClient.ExecuteActionAsync<WorkItemByIterationResponse>(url);

            var (allWorkItemIDs, workItemDict) = GetWorkItemIDsForIteration(response);
            var workItemDetails = (await QueryForworkItems(allWorkItemIDs, projectID)).ToDictionary(k => k.ID);

            foreach (var topLevelWorkItem in workItemDict.Values)
            {
                FillOutWorkItemModel(topLevelWorkItem, workItemDetails[topLevelWorkItem.ID]);

                if (topLevelWorkItem.ChildItems != null)
                {
                    foreach (var childItem in topLevelWorkItem.ChildItems)
                        FillOutWorkItemModel(childItem, workItemDetails[childItem.ID]);
                }
            }

            return workItemDict.Values;
        }

        public async Task<WorkItemModel> GetExpandedWorkItemAsync(string projectID, int workItemID)
        {
            var url = ExpandedRelationsWorkItemDetailsAPIUrl.Replace("{organization}", mSettings.AccountName)
                                                            .Replace("{project}", projectID)
                                                            .Replace("{id}", workItemID.ToString());
            var response = await mClient.ExecuteActionAsync<WorkItemResponse>(url);
            var parent = WorkItemModelFromResponse(response);

            if(response.Relations != null)
            {
                var childWorkItemIDs = new List<int>();

                foreach (var rel in response.Relations)
                {
                    if (rel.Relationship != "System.LinkTypes.Hierarchy-Forward")
                        continue;

                    childWorkItemIDs.Add(ExtractWorkItemIDFromRelationshipURL(rel.URL));
                }

                var childWorkItems = await QueryForworkItems(childWorkItemIDs, projectID);
                parent.ChildItems = childWorkItems.Select(x => WorkItemModelFromResponse(x)).ToList();
            }

            return parent;
        }

        private WorkItemModel WorkItemModelFromResponse(WorkItemResponse response)
        {
            WorkItemModel model = new WorkItemModel();
            FillOutWorkItemModel(model, response);

            return model;
        }

        private void FillOutWorkItemModel(WorkItemModel model, WorkItemResponse response)
        {
            model.Activity = response.Fields.Activity;
            model.CompletedWork = response.Fields.CompletedWork;
            model.IterationPath = response.Fields.IterationPath;
            model.OriginalEstimate = response.Fields.OriginalEstimate;
            model.RemainingWork = response.Fields.RemainingWork;
            model.State = response.Fields.State;
            model.Title = response.Fields.Title;
            model.WorkItemType = response.Fields.WorkItemType;
            model.ID = response.ID;
        }

        private int ExtractWorkItemIDFromRelationshipURL(string url)
        {
            return int.Parse(url.Substring(url.LastIndexOf("/") + 1));
        }

        private async Task<IEnumerable<WorkItemResponse>> QueryForworkItems(IEnumerable<int> workItemIDs, string projectID)
        {
            var urlsToProcess = SplitWorkItemsIntoBatches(workItemIDs, projectID);
            var workItems = new List<WorkItemResponse>();

            foreach (string urlBatch in urlsToProcess)
            {
                var response = await mClient.ExecuteActionAsync<WorkItemListResponse>(urlBatch);
                workItems.AddRange(response.WorkItems);
            }

            return workItems;
        }

        private (IEnumerable<int>, Dictionary<int, WorkItemModel>) GetWorkItemIDsForIteration(WorkItemByIterationResponse response)
        { 
            var workItemDict = new Dictionary<int, WorkItemModel>();
            var allWorkItemIDs = new HashSet<int>();

            foreach(var relation in response.Relations)
            {
                if(relation.Source == null)
                {
                    //top level work item
                    workItemDict.Add(relation.Target.ID, new WorkItemModel() { ID = relation.Target.ID });
                }
                else
                {
                    //Can have User Story -> Bug -> Test Case so we can hit a relation from the bug->test case where we 
                    //don't have an entry for that bug in workItemDict but since we only want user stories in that dictionary
                    //just ignore it
                    if(workItemDict.ContainsKey(relation.Source.ID))
                    {
                        var parent = workItemDict[relation.Source.ID];

                        if (parent.ChildItems == null)
                            parent.ChildItems = new List<WorkItemModel>();

                        parent.ChildItems.Add(new WorkItemModel() { ID = relation.Target.ID });
                    }
                }

                allWorkItemIDs.Add(relation.Target.ID);
            }

            return (allWorkItemIDs, workItemDict);
        }

        private IEnumerable<string> SplitWorkItemsIntoBatches(IEnumerable<int> workItemIDs, string projectID)
        {
            if (!workItemIDs.Any())
                return Enumerable.Empty<string>();

            //Chunk up querying the actual work item IDs into as few batches as possible
            string workItemsURLBase = WorkItemsListAPIUrl.Replace("{organization}", mSettings.AccountName)
                                                         .Replace("{project}", projectID);
            int baseURLLength = workItemsURLBase.Length;
            StringBuilder combinedWorkItemIDs = new StringBuilder();
            List<string> urlsToProcess = new List<string>();

            foreach (int workItemID in workItemIDs)
            {
                string nextChunk = $"{workItemID},";

                if (baseURLLength + combinedWorkItemIDs.Length + nextChunk.Length > 2000)
                {
                    //Remove trailing comma from ids
                    urlsToProcess.Add(workItemsURLBase.Replace("{ids}", combinedWorkItemIDs.Remove(combinedWorkItemIDs.Length - 1, 1).ToString()));
                    combinedWorkItemIDs.Clear();
                }

                combinedWorkItemIDs.Append(nextChunk);
            }

            //Remove trailing comma from ids
            combinedWorkItemIDs.Remove(combinedWorkItemIDs.Length - 1, 1);
            urlsToProcess.Add(workItemsURLBase.Replace("{ids}", combinedWorkItemIDs.ToString()));

            return urlsToProcess;
        }
    }
}
