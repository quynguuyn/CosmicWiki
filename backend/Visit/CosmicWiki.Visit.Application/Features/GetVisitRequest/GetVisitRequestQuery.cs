using CosmicWiki.Visit.Domain.Entities;
using MediatR;
namespace CosmicWiki.Visit.Application.Features.GetVisitRequest;
public sealed record GetVisitRequestQuery(Guid? Id, string? ReferenceCode) : IRequest<VisitRequest?>;
