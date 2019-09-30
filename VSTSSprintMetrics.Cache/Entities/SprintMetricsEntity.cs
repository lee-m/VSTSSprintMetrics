using Microsoft.WindowsAzure.Storage.Table;

namespace VSTSSprintMetrics.Cache.Entities
{
    public class SprintMetricsEntity : TableEntity
    {
        public SprintMetricsEntity()
        { }

        public SprintMetricsEntity(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
        }

        public string MetricsJson { get;set; }
    }
}
