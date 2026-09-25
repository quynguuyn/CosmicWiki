using CosmicWiki.Visit.Application.Features.CreateVisitRequest;
using CosmicWiki.Visit.Domain.Entities;
using FluentValidation;

namespace CosmicWiki.IntegrationTests;

public sealed class VisitRequestContractTests
{
    [Fact]
    public void Create_generates_pending_request_with_normalized_values()
    {
        var request = VisitRequest.Create(
            "  Nguyen Van A ",
            " visitor@example.com ",
            DateTime.UtcNow.AddDays(2),
            3,
            "  Evening observation  ");

        Assert.NotEqual(Guid.Empty, request.Id);
        Assert.StartsWith("OBS-", request.ReferenceCode);
        Assert.Equal("Nguyen Van A", request.Name);
        Assert.Equal("visitor@example.com", request.Email);
        Assert.Equal("Evening observation", request.Notes);
        Assert.Equal(VisitRequestStatus.Pending, request.Status);
    }

    [Fact]
    public void Validator_rejects_invalid_party_size_and_past_date()
    {
        var command = new CreateVisitRequestCommand(
            "Visitor",
            "visitor@example.com",
            DateTime.UtcNow.AddHours(-1),
            0,
            null);

        var result = new CreateVisitRequestValidator().Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(command.RequestedDateUtc));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(command.PartySize));
    }

    [Fact]
    public void Pending_request_can_be_confirmed_then_cancelled()
    {
        var request = VisitRequest.Create("Visitor", "visitor@example.com",
            DateTime.UtcNow.AddDays(1), 2, null);

        request.Confirm();
        Assert.Equal(VisitRequestStatus.Confirmed, request.Status);

        request.Cancel();
        Assert.Equal(VisitRequestStatus.Cancelled, request.Status);
    }

    [Fact]
    public void Terminal_statuses_cannot_be_changed()
    {
        var declined = VisitRequest.Create("Visitor", "visitor@example.com",
            DateTime.UtcNow.AddDays(1), 2, null);
        declined.Decline();

        Assert.Throws<InvalidOperationException>(() => declined.Confirm());
        Assert.Throws<InvalidOperationException>(() => declined.Cancel());

        var cancelled = VisitRequest.Create("Visitor", "visitor@example.com",
            DateTime.UtcNow.AddDays(1), 2, null);
        cancelled.Cancel();

        Assert.Throws<InvalidOperationException>(() => cancelled.Decline());
    }
}
