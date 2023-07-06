using Azure;
using Azure.Data.Tables;

namespace TinyLink.ShortLinks.TableStorage.Entities;

public record ShortLinkTableEntity : ITableEntity
{
    public string RowKey { get; set; }
    public string PartitionKey { get; set; }
    public required string ShortCode { get; set; }
    public required string EndpointUrl { get; set; }
    public DateTimeOffset? ExpiresOn { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}