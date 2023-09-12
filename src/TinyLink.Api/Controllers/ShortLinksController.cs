using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using TinyLink.Api.Controllers.Base;
using TinyLink.ShortLinks.Abstractions.DataTransferObjects;
using TinyLink.ShortLinks.Abstractions.Services;

namespace TinyLink.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ShortLinksController : AuthenticatedControllerBase
{
    private readonly IShortLinksService _shortLinksService;

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> List([FromQuery] string? query, CancellationToken token)
    {
        var ownerId = GetSubjectId();
        var responseObject = await _shortLinksService.ListAsync(ownerId, query, cancellationToken: token);
        return Ok(responseObject);
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Get(Guid id, CancellationToken token)
    {
        var ownerId = GetSubjectId();
        var detailsModel = await _shortLinksService.GetAsync(ownerId, id, token);
        return Ok(detailsModel);
    }

    /// <summary>
    /// Gets whether the short code is unique for the given short link
    /// </summary>
    /// <param name="id">ID of the ShortLink to exclude to prevent false positives on the current ShortLink</param>
    /// <param name="shortCode">Code to check for uniqueness</param>
    /// <param name="token">Continuqation token</param>
    /// <returns></returns>
    [HttpGet("{id:guid}/{shortCode}")]
    [Authorize]
    public async Task<IActionResult> GetIsUnique(Guid id, string shortCode, CancellationToken token)
    {
        var isUnique = await _shortLinksService.IsUniqueShortCodeAsync(id, shortCode, token);
        return isUnique ? Ok() : BadRequest();
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Post(ShortLinkCreateDto dto, CancellationToken token)
    {
        var ownerId = GetSubjectId();
        var responseObject = await _shortLinksService.PostAsync(ownerId, dto.Endpoint, token);
        return Ok(responseObject);
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Put(Guid id, ShortLinkDetailsDto details, CancellationToken token)
    {
        var ownerId = GetSubjectId();
        var success = await _shortLinksService.PutAsync(ownerId, id, details, token);
        if (success)
        {
            return Ok();
        }

        return BadRequest();
    }

    [HttpPatch("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Patch(Guid id, JsonPatchDocument<ShortLinkDetailsDto> patchDocument, CancellationToken token)
    {
        var ownerId = GetSubjectId();
        var detailsModel = await _shortLinksService.GetAsync(ownerId, id, token);
        patchDocument.ApplyTo(detailsModel, ModelState);
        var success = await _shortLinksService.PutAsync(ownerId, id, detailsModel, token);
        if (success)
        {
            return Ok();
        }

        return BadRequest();
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id, CancellationToken token)
    {
        var ownerId = GetSubjectId();
        var succeeded = await _shortLinksService.DeleteAsync(ownerId, id, token);
        return succeeded ? Ok() : NotFound();
    }

    public ShortLinksController(IShortLinksService shortLinksService)
    {
        _shortLinksService = shortLinksService;
    }
}
