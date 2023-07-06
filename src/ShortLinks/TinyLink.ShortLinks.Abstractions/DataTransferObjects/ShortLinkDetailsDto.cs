namespace TinyLink.ShortLinks.Abstractions.DataTransferObjects;

public class ShortLinkDetailsDto
{
    public required Guid Id { get; init; }
    public required string ShortCode { get; init; }
    public required string EndpointUrl { get; init; }
    public DateTimeOffset? CreatedOn { get; init; }
    public DateTimeOffset? ExpiresOn { get; init; }
}
