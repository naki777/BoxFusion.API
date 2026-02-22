// Models/Category.cs
public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public ICollection<Product> Products { get; set; } = new List<Product>();
}

// Models/Product.cs
public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Info { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Count { get; set; }
    public string? Image { get; set; }

    // კავშირები
    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    public string CreatedByUserId { get; set; } = string.Empty; // ვინ დაამატა
}