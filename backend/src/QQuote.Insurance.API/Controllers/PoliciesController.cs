using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QQuote.Insurance.Application.Services;

namespace QQuote.Insurance.API.Controllers;

[ApiController]
[Authorize]
[Route("api/policies")]
public class PoliciesController : ControllerBase
{
    private readonly PolicyAppService _policies;
    public PoliciesController(PolicyAppService policies) => _policies = policies;

    [HttpPost("{quoteId:guid}/convert")]
    public async Task<IActionResult> Convert(Guid quoteId, CancellationToken ct)
    {
        var policyId = await _policies.ConvertAsync(quoteId, ct);
        return Ok(new { policyId });
    }
}
