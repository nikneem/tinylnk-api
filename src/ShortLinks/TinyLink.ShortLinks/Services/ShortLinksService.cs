using System.Text.RegularExpressions;
using HexMaster.DomainDrivenDesign;
using TinyLink.Core;
using TinyLink.Core.Abstractions;
using TinyLink.Core.Abstractions.Commands;
using TinyLink.Core.Commands;
using TinyLink.Core.Commands.CommandMessages;
using TinyLink.Core.Helpers;
using TinyLink.ShortLinks.Abstractions.DataTransferObjects;
using TinyLink.ShortLinks.Abstractions.DomainModels;
using TinyLink.ShortLinks.Abstractions.ErrorCodes;
using TinyLink.ShortLinks.Abstractions.Exceptions;
using TinyLink.ShortLinks.Abstractions.Repositories;
using TinyLink.ShortLinks.Abstractions.Services;
using TinyLink.ShortLinks.DomainModels;

namespace TinyLink.ShortLinks.Services;

public class ShortLinksService : IShortLinksService
{
    private readonly IShortLinksRepository _repository;
    private readonly ICommandsSenderFactory _commandsSenderFactory;

    public Task<List<ShortLinksListItemDto>> ListAsync(string ownerId, string? query, CancellationToken cancellationToken = default)
    {
        // Sanitize pagination input and throw exceptions for invalid values
        //Sanitize.PaginationInput(page, pageSize);

        if (!string.IsNullOrWhiteSpace(query) && (!Regex.IsMatch(query, Constants.AlphanumericStringRegularExpression)))
        {
            throw new UrlShortnerShortLinkException(UrlShortnerShortLinksErrorCodes.QueryStringInvalid);
        }

        return _repository.ListAsync(ownerId, query, cancellationToken);
    }
    public Task<ShortLinkDetailsDto> GetAsync(string ownerId, Guid id, CancellationToken cancellationToken = default)
    {
        return _repository.GetAsync(ownerId, id, cancellationToken);
    }
    public async Task<ShortLinkDetailsDto> PostAsync(string ownerId, string targetUrl, CancellationToken cancellationToken = default)
    {
        var uniqueCode = await  GenerateUniqueShortCodeAsync(cancellationToken);
        var domainModel = ShortLink.Create(targetUrl, uniqueCode);
        if (await _repository.UpdateAsync(ownerId, domainModel, cancellationToken))
        {
            return DomainModelToDto(domainModel);
        }
        throw new UrlShortnerShortLinkException(UrlShortnerShortLinksErrorCodes.ShortCodeCreationFailed);
    }
    public async Task<bool> PutAsync(string ownerId, Guid id, ShortLinkDetailsDto dto, CancellationToken cancellationToken = default)
    {
        var domainModel = await _repository.GetDomainModelAsync(ownerId, id, cancellationToken);
        await domainModel.SetShortCode(dto.ShortCode, shortCode => IsUniqueShortCodeAsync(id, shortCode, cancellationToken));
        domainModel.SetTargetUrl(dto.EndpointUrl);
        domainModel.SetExpiryDate(dto.ExpiresOn);
        return await _repository.UpdateAsync(ownerId, domainModel, cancellationToken);
    }

    public async Task<ShortLinkDetailsDto> ResolveAsync(string shortCode, CancellationToken cancellationToken = default)
    {
        var domainModel = await _repository.ResolveAsync(shortCode, cancellationToken);
        await _commandsSenderFactory.Send(
            new ProcessHitCommand(shortCode, DateTimeOffset.UtcNow),
            QueueName.HitsQueueName);
        return DomainModelToDto(domainModel);
    }

    public async Task<bool> IsUniqueShortCodeAsync( Guid id, string shortCode, CancellationToken cancellationToken = default)
    {
        return !await _repository.ExistsAsync(id, shortCode, cancellationToken);
    }

    private async Task<string> GenerateUniqueShortCodeAsync(CancellationToken cancellationToken = default)
    {
        var shortCode = Randomizer.GetRandomShortCode();
        var exists = await _repository.ExistsAsync(Guid.Empty, shortCode, cancellationToken);
        if (exists)
        {
            return await GenerateUniqueShortCodeAsync(cancellationToken);
        }

        return shortCode;
    }
    private static ShortLinkDetailsDto DomainModelToDto(IShortLink domainModel)
    {
        if (domainModel is DomainModel<Guid> dm)
        {
            return new ShortLinkDetailsDto
            {
                Id = dm.Id,
                ShortCode = domainModel.ShortCode,
                EndpointUrl = domainModel.TargetUrl,
                CreatedOn = domainModel.CreatedOn,
                ExpiresOn = domainModel.ExpiresOn
            };
        }
        return  null!;
    }

    public ShortLinksService(IShortLinksRepository repository, ICommandsSenderFactory commandsSenderFactory)
    {
        _repository = repository;
        _commandsSenderFactory = commandsSenderFactory;
    }

}