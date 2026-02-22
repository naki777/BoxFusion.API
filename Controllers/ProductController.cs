using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using BoxFusion.API.BoxFusion.Domain.Entities;
using BoxFusion.Application.DTOs;

namespace BoxFusion.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly BoxFusionDbContext _context;

    public ProductController(BoxFusionDbContext context)
    {
        _context = context;
    }

    // GET: api/product
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var products = await _context.Products
            .Include(p => p.Category)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Info = p.Info,
                Price = p.Price,
                Count = p.Count,
                Image = p.Image,
                CategoryId = p.CategoryId,
                CategoryName = p.Category != null ? p.Category.Name : null
            })
            .ToListAsync();

        return Ok(products);
    }

    // GET: api/product/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .Where(p => p.Id == id)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Info = p.Info,
                Price = p.Price,
                Count = p.Count,
                Image = p.Image,
                CategoryId = p.CategoryId,
                CategoryName = p.Category != null ? p.Category.Name : null
            })
            .FirstOrDefaultAsync();

        if (product == null)
            return NotFound("პროდუქტი ვერ მოიძებნა");

        return Ok(product);
    }

    // POST: api/product
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
    {
        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId);
        if (!categoryExists)
            return BadRequest("კატეგორია ვერ მოიძებნა");

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        var product = new Product
        {
            Name = dto.Name,
            Info = dto.Info,
            Price = dto.Price,
            Count = dto.Count,
            Image = dto.Image,
            CategoryId = dto.CategoryId,
            CreatedByUserId = userId
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = product.Id }, new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Info = product.Info,
            Price = product.Price,
            Count = product.Count,
            Image = product.Image,
            CategoryId = product.CategoryId
        });
    }

    // PUT: api/product/5
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateProductDto dto)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return NotFound("პროდუქტი ვერ მოიძებნა");

        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId);
        if (!categoryExists)
            return BadRequest("კატეგორია ვერ მოიძებნა");

        product.Name = dto.Name;
        product.Info = dto.Info;
        product.Price = dto.Price;
        product.Count = dto.Count;
        product.Image = dto.Image;
        product.CategoryId = dto.CategoryId;

        await _context.SaveChangesAsync();

        return Ok(new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Info = product.Info,
            Price = product.Price,
            Count = product.Count,
            Image = product.Image,
            CategoryId = product.CategoryId
        });
    }

    // DELETE: api/product/5
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return NotFound("პროდუქტი ვერ მოიძებნა");

        product.IsDeleted = true; // Soft Delete!
        await _context.SaveChangesAsync();

        return NoContent();
    }
}