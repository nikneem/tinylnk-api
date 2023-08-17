using TinyLink.Hits.Abstractions.DataTransferObjects;

namespace TinyLink.Hits.Abstractions.Services;

public interface IHitsService
{
    Task<HitsTotalDto> GetHitsTotalAsync(
        string shortCode, 
        string ownerId,
        CancellationToken cancellationToken = default);
}