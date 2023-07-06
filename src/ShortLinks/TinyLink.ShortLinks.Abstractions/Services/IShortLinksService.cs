using TinyLink.ShortLinks.Abstractions.DataTransferObjects;

namespace TinyLink.ShortLinks.Abstractions.Services;

public interface IShortLinksService
{
    Task<List<ShortLinksListItemDto>> ListAsync(string ownerId, string? query, CancellationToken cancellationToken = default);
    Task<ShortLinkDetailsDto> GetAsync(string ownerId, Guid id, CancellationToken cancellationToken = default);
    Task<ShortLinkDetailsDto> PostAsync(string ownerId, string targetUrl, CancellationToken cancellationToken = default);
    Task<bool> PutAsync(string ownerId, Guid id, ShortLinkDetailsDto dto, CancellationToken cancellationToken = default);
    Task<ShortLinkDetailsDto> ResolveAsync(string shortCode, CancellationToken cancellationToken = default);
    Task<bool> IsUniqueShortCodeAsync(Guid id, string shortCode, CancellationToken cancellationToken = default);
}
