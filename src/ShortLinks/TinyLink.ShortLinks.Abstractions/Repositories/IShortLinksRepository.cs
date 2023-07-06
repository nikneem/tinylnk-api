using TinyLink.ShortLinks.Abstractions.DataTransferObjects;
using TinyLink.ShortLinks.Abstractions.DomainModels;

namespace TinyLink.ShortLinks.Abstractions.Repositories;

public interface IShortLinksRepository
{
    Task<List<ShortLinksListItemDto>> ListAsync(string ownerId, string? query, CancellationToken cancellationToken);
    Task<ShortLinkDetailsDto> GetAsync(string ownerId, Guid id, CancellationToken cancellationToken);
    Task<IShortLink> GetDomainModelAsync(string ownerId, Guid id, CancellationToken cancellationToken);
    Task<bool> UpdateAsync(string ownerId, IShortLink domainModel, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(Guid id, string shortCode, CancellationToken cancellationToken);
    Task<IShortLink> ResolveAsync(string shortCode, CancellationToken cancellationToken);
}
