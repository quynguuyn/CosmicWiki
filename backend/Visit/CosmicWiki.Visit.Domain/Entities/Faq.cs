using CosmicWiki.SharedKernel.Abstractions;

namespace CosmicWiki.Visit.Domain.Entities;

public sealed class Faq : AggregateRoot<Guid>
{
    private Faq(
        Guid id,
        string question,
        string answer,
        int displayOrder,
        bool isPublished) : base(id)
    {
        Question = question;
        Answer = answer;
        DisplayOrder = displayOrder;
        IsPublished = isPublished;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = CreatedAtUtc;
    }

    private Faq() : base(Guid.Empty) { }

    public string Question { get; private set; } = null!;
    public string Answer { get; private set; } = null!;
    public int DisplayOrder { get; private set; }
    public bool IsPublished { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    public void Update(string question, string answer, int displayOrder, bool isPublished)
    {
        Question = question.Trim();
        Answer = answer.Trim();
        DisplayOrder = displayOrder;
        IsPublished = isPublished;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public static Faq Create(
        string question,
        string answer,
        int displayOrder,
        bool isPublished) =>
        new(Guid.NewGuid(), question.Trim(), answer.Trim(), displayOrder, isPublished);
}
