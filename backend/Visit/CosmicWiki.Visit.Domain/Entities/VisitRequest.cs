using CosmicWiki.SharedKernel.Abstractions;

namespace CosmicWiki.Visit.Domain.Entities;

public enum VisitRequestStatus { Pending = 0, Confirmed = 1, Declined = 2, Cancelled = 3 }

public sealed class VisitRequest : AggregateRoot<Guid>
{
    private VisitRequest(Guid id, string referenceCode, string name, string email,
        DateTime requestedDateUtc, int partySize, string? notes) : base(id)
    {
        ReferenceCode = referenceCode; Name = name; Email = email;
        RequestedDateUtc = requestedDateUtc; PartySize = partySize; Notes = notes;
        Status = VisitRequestStatus.Pending; CreatedAtUtc = DateTime.UtcNow;
    }
    private VisitRequest() : base(Guid.Empty) { }
    public string ReferenceCode { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public DateTime RequestedDateUtc { get; private set; }
    public int PartySize { get; private set; }
    public string? Notes { get; private set; }
    public VisitRequestStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    public void Confirm()
    {
        EnsurePending("confirm");
        Status = VisitRequestStatus.Confirmed;
    }

    public void Decline()
    {
        EnsurePending("decline");
        Status = VisitRequestStatus.Declined;
    }

    public void Cancel()
    {
        if (Status is not (VisitRequestStatus.Pending or VisitRequestStatus.Confirmed))
            throw new InvalidOperationException(
                $"A visit request in {Status} status cannot be cancelled.");

        Status = VisitRequestStatus.Cancelled;
    }

    private void EnsurePending(string transition)
    {
        if (Status != VisitRequestStatus.Pending)
            throw new InvalidOperationException(
                $"A visit request in {Status} status cannot be {transition}d.");
    }

    public static VisitRequest Create(string name, string email, DateTime requestedDateUtc, int partySize, string? notes) =>
        new(Guid.NewGuid(), $"OBS-{Guid.NewGuid():N}".ToUpperInvariant()[..16],
            name.Trim(), email.Trim(), requestedDateUtc, partySize,
            string.IsNullOrWhiteSpace(notes) ? null : notes.Trim());
}
