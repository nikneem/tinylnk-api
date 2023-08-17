using TinyLink.Hits.Abstractions.DataTransferObjects;
using TinyLink.Hits.Abstractions.Repositories;
using TinyLink.Hits.Abstractions.Services;

namespace TinyLink.Hits.Services;

public class HitsService : IHitsService
{
    private readonly IHitsTotalRepository _hitsTotalRepository;

    public Task<HitsTotalDto> GetHitsTotalAsync(string shortCode, string ownerId, CancellationToken cancellationToken = default)
    {
        return _hitsTotalRepository.GetAsync(ownerId, shortCode, cancellationToken);
    }

    public HitsService(IHitsTotalRepository hitsTotalRepository)
    {
        _hitsTotalRepository = hitsTotalRepository;
    }
}