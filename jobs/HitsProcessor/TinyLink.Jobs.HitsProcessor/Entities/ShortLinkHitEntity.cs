using Azure;
using Azure.Data.Tables;

namespace TinyLink.Jobs.HitsProcessor.Entities;

public class ShortLinkHitEntity : ITableEntity
{
    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public string ShortCode { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}