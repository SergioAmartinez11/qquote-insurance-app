using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QQuote.Insurance.Application.Common.Interfaces;
using QQuote.Insurance.Application.DTOs;
using QQuote.Insurance.Application.Services;

namespace QQuote.Insurance.API.Controllers;

[ApiController]
[Authorize]
[Route("api/quotes")]
public class QuotesController : ControllerBase
{
    private readonly QuoteAppService     _quotes;
    private readonly ICurrentUserService _currentUser;

    public QuotesController(QuoteAppService quotes, ICurrentUserService currentUser)
    {
        _quotes      = quotes;
        _currentUser = currentUser;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateQuoteRequest request, CancellationToken ct)
    {
        var result = await _quotes.CreateAsync(_currentUser.CustomerId, request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _quotes.GetByIdAsync(id, ct);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetMyQuotes(CancellationToken ct)
    {
        var result = await _quotes.GetByCustomerAsync(_currentUser.CustomerId, ct);
        return Ok(result);
    }
}
