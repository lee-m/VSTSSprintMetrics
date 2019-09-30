using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

using Newtonsoft.Json;

using VSTSSprintMetrics.Cache.Entities;
using VSTSSprintMetrics.Core.Interfaces;
using VSTSSprintMetrics.Core.Models;

namespace VSTSSprintMetrics.Cache
{
    public class AzureTableStorageDataCache : IDataCache
    {
        private readonly CloudTableClient mClient;

        private const string SprintMetricsTableName = "SprintMetrics";
        private const string CrossSprintMetricsTableName = "CrossSprintMetrics";

        public AzureTableStorageDataCache(IOptions<StorageAccountSettings> options)
        {
            string connectionString = 
                $"DefaultEndpointsProtocol=https;AccountName={options.Value.AccountName};AccountKey={options.Value.AccountKey};EndpointSuffix=core.windows.net";

            CloudStorageAccount account = CloudStorageAccount.Parse(connectionString);
            mClient = account.CreateCloudTableClient();
        }

        public async Task<IEnumerable<WorkItemMetricsModel>> GetSprintMetricsAsync(string teamID, string iterationID)
        {
            return await GetCacheItem<IEnumerable<WorkItemMetricsModel>>(SprintMetricsTable, teamID, iterationID);
        }

        public async Task InsertSprintMetricsAsync(string teamID, string iterationID, IEnumerable<WorkItemMetricsModel> metrics)
        {
            var newEntity = new SprintMetricsEntity(teamID, iterationID)
            {
                MetricsJson = JsonConvert.SerializeObject(metrics)
            };

            var operation = TableOperation.InsertOrReplace(newEntity);
            await SprintMetricsTable.ExecuteAsync(operation);
        }

        public async Task BulkInsertCrossSprintMetrics(string iterationID, IEnumerable<CrossSprintMetricsModel> metrics)
        {
            foreach(var metricsItem in metrics)
            {
                var entity = new SprintMetricsEntity(iterationID, metricsItem.WorkItemID.ToString())
                {
                    MetricsJson = JsonConvert.SerializeObject(metricsItem)
                };

                var op = TableOperation.InsertOrReplace(entity);
                await CrossSprintMetricsTable.ExecuteAsync(op);
            }
        }

        public async Task<CrossSprintMetricsModel> GetCrossSprintMetrics(string iterationID, string workItemID)
        {
            return await GetCacheItem<CrossSprintMetricsModel>(CrossSprintMetricsTable, iterationID, workItemID);
        }

        private async Task<T> GetCacheItem<T>(CloudTable table, string partitionKey, string rowKey)
        {
            var operation = TableOperation.Retrieve<SprintMetricsEntity>(partitionKey, rowKey);
            var result = await table.ExecuteAsync(operation);

            if (result.Result != null)
            {
                var content = ((SprintMetricsEntity)result.Result).MetricsJson;
                return JsonConvert.DeserializeObject<T>(content);
            }

            return default(T);
        }

        private CloudTable SprintMetricsTable => mClient.GetTableReference(SprintMetricsTableName);
        private CloudTable CrossSprintMetricsTable => mClient.GetTableReference(CrossSprintMetricsTableName);
    }
}
