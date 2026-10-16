
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedcodeBookApi.Data;
using RedcodeBookApi.Models;

namespace RedcodeBookApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class BooksController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public BooksController(ApplicationDbContext context)
    {
        _context = context;
    }

    private string? UserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub");

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Book>>> GetBooks()
    {
        if (UserId == null) return Unauthorized();

        return await _context.Books
            .AsNoTracking()
            .Where(b => b.UserId == UserId)
            .OrderByDescending(b => b.Id)
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Book>> GetBook(int id)
    {
        if (UserId == null) return Unauthorized();

        var book = await _context.Books
            .FirstOrDefaultAsync(b =>
                b.Id == id && b.UserId == UserId);

        if (book == null) return NotFound();

        return book;
    }

    [HttpPost]
    public async Task<ActionResult<Book>> CreateBook(Book book)
    {
        if (UserId == null) return Unauthorized();

        if (string.IsNullOrWhiteSpace(book.Title) ||
            string.IsNullOrWhiteSpace(book.Author))
            return BadRequest("Titel och författare krävs.");

        book.Id = 0;
        book.UserId = UserId;
        book.PublishedDate = ToUtc(book.PublishedDate);

        _context.Books.Add(book);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetBook),
            new { id = book.Id },
            book);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBook(int id, Book book)
    {
        if (UserId == null) return Unauthorized();

        if (id != book.Id)
            return BadRequest("Fel bok-ID.");

        if (string.IsNullOrWhiteSpace(book.Title) ||
            string.IsNullOrWhiteSpace(book.Author))
            return BadRequest("Titel och författare krävs.");

        var existingBook = await _context.Books
            .FirstOrDefaultAsync(b =>
                b.Id == id && b.UserId == UserId);

        if (existingBook == null) return NotFound();

        existingBook.Title = book.Title;
        existingBook.Author = book.Author;
        existingBook.PublishedDate = ToUtc(book.PublishedDate);

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBook(int id)
    {
        if (UserId == null) return Unauthorized();

        var book = await _context.Books
            .FirstOrDefaultAsync(b =>
                b.Id == id && b.UserId == UserId);

        if (book == null) return NotFound();

        _context.Books.Remove(book);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static DateTime ToUtc(DateTime date)
    {
        return date.Kind switch
        {
            DateTimeKind.Utc => date,
            DateTimeKind.Local => date.ToUniversalTime(),
            _ => DateTime.SpecifyKind(date, DateTimeKind.Utc)
        };
    }
}
