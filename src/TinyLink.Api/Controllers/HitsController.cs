using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TinyLink.Api.Controllers.Base;
using TinyLink.Hits.Abstractions.Services;

namespace TinyLink.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HitsController : AuthenticatedControllerBase
    {
        private readonly IHitsService _hitsService;

        [HttpGet("{shortCode}/total")]
        public async Task<IActionResult> GetTotal(string shortCode, CancellationToken cancellationToken)
        {
            var ownerId = GetSubjectId();
            var responseDto = await _hitsService.GetHitsTotalAsync(shortCode, ownerId, cancellationToken);
            return Ok(responseDto);
        }

        public HitsController(IHitsService hitsService)
        {
            _hitsService = hitsService;
        }
    }
}
