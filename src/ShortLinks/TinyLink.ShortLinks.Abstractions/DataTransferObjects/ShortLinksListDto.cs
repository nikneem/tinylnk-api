namespace TinyLink.ShortLinks.Abstractions.DataTransferObjects;

public class ShortLinksListDto
{

    public required int Page { get; init; }
    public required int PageSize { get; init; }
    public required int TotalPages { get; init; }
    public required int TotalRecords { get; init; }

    public required List<ShortLinksListItemDto> ShortLinks { get; init; }

}
