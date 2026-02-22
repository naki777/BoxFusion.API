namespace BoxFusion.Application.DTOs;

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Info { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Count { get; set; }
    public string? Image { get; set; }
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }
}

public class CreateProductDto
{
    public string Name { get; set; } = string.Empty;
    public string Info { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Count { get; set; }
    public string? Image { get; set; }
    public int CategoryId { get; set; }
}