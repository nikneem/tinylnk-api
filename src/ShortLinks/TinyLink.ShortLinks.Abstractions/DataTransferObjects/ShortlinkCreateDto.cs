namespace TinyLink.ShortLinks.Abstractions.DataTransferObjects;

public record ShortLinkCreateDto
{
    public required string Endpoint { get; init; }
}