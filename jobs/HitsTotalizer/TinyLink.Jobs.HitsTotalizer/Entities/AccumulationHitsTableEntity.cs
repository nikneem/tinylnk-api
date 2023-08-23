using Azure;
using Azure.Data.Tables;

namespace TinyLink.Jobs.HitsTotalizer.Entities;

public class AccumulationHitsTableEntity : ITableEntity
{
    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public string ShortCode { get; set; }
    public string OwnerId { get; set; }
    public int CumulatedCount { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}