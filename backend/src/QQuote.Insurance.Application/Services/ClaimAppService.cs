using QQuote.Insurance.Application.DTOs;
using QQuote.Insurance.Domain.Entities;
using QQuote.Insurance.Domain.Interfaces;

namespace QQuote.Insurance.Application.Services;

public class ClaimAppService
{
    private readonly IClaimRepository _claimRepo;

    public ClaimAppService(IClaimRepository claimRepo) => _claimRepo = claimRepo;

    public async Task<ClaimResponse> FileAsync(
        CreateClaimRequest request,
        CancellationToken  ct = default)
    {
        var claim = Claim.File(request.PolicyId, request.IncidentDate, request.Description);
        await _claimRepo.AddAsync(claim, ct);
        await _claimRepo.SaveChangesAsync(ct);
        return MapToResponse(claim);
    }

    public async Task<List<ClaimResponse>> GetByPolicyAsync(
        Guid policyId, CancellationToken ct = default)
    {
        var claims = await _claimRepo.GetByPolicyIdAsync(policyId, ct);
        return claims.Select(MapToResponse).ToList();
    }

    private static ClaimResponse MapToResponse(Claim c) =>
        new(c.Id, c.PolicyId, c.IncidentDate, c.Description, c.Status.ToString(), c.CreatedAt);
}
