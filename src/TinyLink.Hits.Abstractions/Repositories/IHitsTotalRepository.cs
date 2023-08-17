using TinyLink.Hits.Abstractions.DataTransferObjects;

namespace TinyLink.Hits.Abstractions.Repositories;

public interface IHitsTotalRepository
{
    Task<HitsTotalDto> GetAsync(string ownerId, string shortCode, CancellationToken cancellationToken);
}