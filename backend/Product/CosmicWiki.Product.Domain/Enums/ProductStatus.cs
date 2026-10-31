namespace CosmicWiki.Product.Domain.Enums;

public enum ProductStatus
{
    Draft = 1,
    Active = 2,
    Published = Active,
    Inactive = 3,
    Archived = Inactive
}
