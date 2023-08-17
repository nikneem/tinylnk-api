using Azure;
using Azure.Data.Tables;

namespace TinyLink.Hits.TableStorage.Entities;

public record HitsTotalTableEntity : ITableEntity
{
    public string RowKey { get; set; }
    public string PartitionKey { get; set; }
    public required string OwnerId{ get; set; }
    public required int Hits { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}