using MediatR;
namespace CosmicWiki.Visit.Application.Features.CreateVisitRequest;
public sealed record CreateVisitRequestCommand(string Name, string Email, DateTime RequestedDateUtc, int PartySize, string? Notes) : IRequest<Guid>;
