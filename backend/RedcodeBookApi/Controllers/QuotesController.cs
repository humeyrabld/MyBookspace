
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedcodeBookApi.Data;
using RedcodeBookApi.Models;

namespace RedcodeBookApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class QuotesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public QuotesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Hämta den inloggade användarens ID
    private string? UserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub");

    // GET: api/quotes
    // Visa endast användarens egna citat
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Quote>>> GetQuotes()
    {
        var userId = UserId;

        if (userId == null)
            return Unauthorized();

        return await _context.Quotes
            .AsNoTracking()
            .Where(q => q.UserId == userId)
            .OrderByDescending(q => q.Id)
            .ToListAsync();
    }

    // GET: api/quotes/1
    // Visa ett specifikt citat
    [HttpGet("{id}")]
    public async Task<ActionResult<Quote>> GetQuote(int id)
    {
        var userId = UserId;

        if (userId == null)
            return Unauthorized();

        var quote = await _context.Quotes
            .AsNoTracking()
            .FirstOrDefaultAsync(q =>
                q.Id == id && q.UserId == userId);

        if (quote == null)
            return NotFound();

        return quote;
    }

    // POST: api/quotes
    // Skapa ett nytt personligt citat
    [HttpPost]
    public async Task<ActionResult<Quote>> CreateQuote(Quote quote)
    {
        var userId = UserId;

        if (userId == null)
            return Unauthorized();

        if (string.IsNullOrWhiteSpace(quote.Text))
            return BadRequest("Citatet får inte vara tomt.");

        quote.Id = 0;
        quote.UserId = userId;
        quote.Text = quote.Text.Trim();
        quote.Author = quote.Author?.Trim() ?? string.Empty;

        _context.Quotes.Add(quote);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetQuote),
            new { id = quote.Id },
            quote
        );
    }

    // PUT: api/quotes/1
    // Redigera endast egna citat
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateQuote(int id, Quote quote)
    {
        var userId = UserId;

        if (userId == null)
            return Unauthorized();

        if (id != quote.Id)
            return BadRequest("Fel citat-ID.");

        if (string.IsNullOrWhiteSpace(quote.Text))
            return BadRequest("Citatet får inte vara tomt.");

        var existingQuote = await _context.Quotes
            .FirstOrDefaultAsync(q =>
                q.Id == id && q.UserId == userId);

        if (existingQuote == null)
            return NotFound();

        existingQuote.Text = quote.Text.Trim();
        existingQuote.Author = quote.Author?.Trim() ?? string.Empty;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/quotes/1
    // Radera endast egna citat
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteQuote(int id)
    {
        var userId = UserId;

        if (userId == null)
            return Unauthorized();

        var quote = await _context.Quotes
            .FirstOrDefaultAsync(q =>
                q.Id == id && q.UserId == userId);

        if (quote == null)
            return NotFound();

        _context.Quotes.Remove(quote);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

