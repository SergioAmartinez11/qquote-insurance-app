using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QQuote.Insurance.Application.Common.Interfaces;
using QQuote.Insurance.Application.Services;

namespace QQuote.Insurance.API.Controllers;

[ApiController]
[Authorize]
[Route("api/policies")]
public class PoliciesController : ControllerBase
{
    private readonly PolicyAppService    _policies;
    private readonly ICurrentUserService _currentUser;

    public PoliciesController(PolicyAppService policies, ICurrentUserService currentUser)
    {
        _policies    = policies;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyPolicies(CancellationToken ct)
    {
        var result = await _policies.GetMyPoliciesAsync(_currentUser.CustomerId, ct);
        return Ok(result);
    }

    [HttpPost("{quoteId:guid}/convert")]
    public async Task<IActionResult> Convert(Guid quoteId, CancellationToken ct)
    {
        var policyId = await _policies.ConvertAsync(quoteId, ct);
        return Ok(new { policyId });
    }
}
