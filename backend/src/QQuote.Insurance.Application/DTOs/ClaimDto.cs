namespace QQuote.Insurance.Application.DTOs;

public record CreateClaimRequest(
    Guid     PolicyId,
    DateTime IncidentDate,
    string   Description);

public record ClaimResponse(
    Guid     Id,
    Guid     PolicyId,
    DateTime IncidentDate,
    string   Description,
    string   Status,
    DateTime CreatedAt);
