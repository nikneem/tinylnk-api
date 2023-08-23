using Azure;
using Azure.Data.Tables;

namespace TinyLink.Hits.TableStorage.Entities;

public record HitTableEntity : ITableEntity
{
    public string PartitionKey { get; set; } // Can be hit, accumulation, total or processed
    public string RowKey { get; set; }
    public required string ShortCode { get; set; }
    public required string OwnerId { get; set; }
    public required int Hits { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}