using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StreamingApp.Application.DTOs.Content;
using StreamingApp.Infrastructure.Data;

namespace StreamingApp.API.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoryController : ControllerBase
{
    private readonly AppDbContext _context;

    public CategoryController(AppDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var categories = await _context.Categories
            .Select(c => new CategoryDto(c.Id, c.Name, c.Slug, c.IconName))
            .ToListAsync(ct);
        return Ok(categories);
    }
}
