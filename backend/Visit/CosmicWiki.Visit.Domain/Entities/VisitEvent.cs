using CosmicWiki.SharedKernel.Abstractions;

namespace CosmicWiki.Visit.Domain.Entities;

public sealed class VisitEvent : AggregateRoot<Guid>
{
    private VisitEvent(
        Guid id,
        DateTime eventDateUtc,
        string name,
        string description,
        string? specification,
        string? location,
        string status,
        string? statusType,
        string? action,
        int displayOrder,
        bool isPublished) : base(id)
    {
        EventDateUtc = eventDateUtc;
        Name = name;
        Description = description;
        Specification = specification;
        Location = location;
        Status = status;
        StatusType = statusType;
        Action = action;
        DisplayOrder = displayOrder;
        IsPublished = isPublished;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = CreatedAtUtc;
    }

    private VisitEvent() : base(Guid.Empty) { }

    public DateTime EventDateUtc { get; private set; }
    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public string? Specification { get; private set; }
    public string? Location { get; private set; }
    public string Status { get; private set; } = null!;
    public string? StatusType { get; private set; }
    public string? Action { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsPublished { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    public void Update(
        DateTime eventDateUtc,
        string name,
        string description,
        string? specification,
        string? location,
        string status,
        string? statusType,
        string? action,
        int displayOrder,
        bool isPublished)
    {
        EventDateUtc = eventDateUtc;
        Name = name.Trim();
        Description = description.Trim();
        Specification = TrimNullable(specification);
        Location = TrimNullable(location);
        Status = status.Trim();
        StatusType = TrimNullable(statusType);
        Action = TrimNullable(action);
        DisplayOrder = displayOrder;
        IsPublished = isPublished;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public static VisitEvent Create(
        DateTime eventDateUtc,
        string name,
        string description,
        string? specification,
        string? location,
        string status,
        string? statusType,
        string? action,
        int displayOrder,
        bool isPublished) =>
        new(
            Guid.NewGuid(),
            eventDateUtc,
            name.Trim(),
            description.Trim(),
            TrimNullable(specification),
            TrimNullable(location),
            status.Trim(),
            TrimNullable(statusType),
            TrimNullable(action),
            displayOrder,
            isPublished);

    private static string? TrimNullable(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
