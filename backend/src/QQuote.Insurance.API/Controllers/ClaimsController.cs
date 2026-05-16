using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QQuote.Insurance.Application.DTOs;
using QQuote.Insurance.Application.Services;

namespace QQuote.Insurance.API.Controllers;

[ApiController]
[Authorize]
[Route("api/claims")]
public class ClaimsController : ControllerBase
{
    private readonly ClaimAppService _claims;
    public ClaimsController(ClaimAppService claims) => _claims = claims;

    [HttpPost]
    public async Task<IActionResult> File(
        [FromBody] CreateClaimRequest request, CancellationToken ct)
    {
        var result = await _claims.FileAsync(request, ct);
        return Created($"/api/claims/{result.Id}", result);
    }

    [HttpGet("policy/{policyId:guid}")]
    public async Task<IActionResult> GetByPolicy(Guid policyId, CancellationToken ct)
    {
        var result = await _claims.GetByPolicyAsync(policyId, ct);
        return Ok(result);
    }
}
