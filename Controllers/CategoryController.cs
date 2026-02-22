using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BoxFusion.API.BoxFusion.Domain.Entities;
using BoxFusion.Application.DTOs;

namespace BoxFusion.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoryController : ControllerBase
{
    private readonly BoxFusionDbContext _context;

    public CategoryController(BoxFusionDbContext context)
    {
        _context = context;
    }

    // GET: api/category
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _context.Categories
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Icon = c.Icon
            })
            .ToListAsync();

        return Ok(categories);
    }

    // GET: api/category/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var category = await _context.Categories
            .Where(c => c.Id == id)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Icon = c.Icon
            })
            .FirstOrDefaultAsync();

        if (category == null)
            return NotFound("კატეგორია ვერ მოიძებნა");

        return Ok(category);
    }

    // POST: api/category
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto)
    {
        var category = new Category
        {
            Name = dto.Name,
            Icon = dto.Icon
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = category.Id }, new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Icon = category.Icon
        });
    }


    // PUT: api/category/5
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateCategoryDto dto)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
            return NotFound("კატეგორია ვერ მოიძებნა");

        category.Name = dto.Name;
        category.Icon = dto.Icon;

        await _context.SaveChangesAsync();

        return Ok(new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Icon = category.Icon
        });
    }

    // DELETE: api/category/5
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
            return NotFound("კატეგორია ვერ მოიძებნა");

        category.IsDeleted = true; // Soft Delete!
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

