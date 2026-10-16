using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RedcodeBookApi.Data;
using RedcodeBookApi.DTOs;
using RedcodeBookApi.Models;

namespace RedcodeBookApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private const string CookieName = "bookspace_refresh";
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthController(UserManager<IdentityUser> userManager, ApplicationDbContext context, IConfiguration configuration)
    {
        _userManager = userManager;
        _context = context;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var user = new IdentityUser { UserName = dto.Email, Email = dto.Email };
        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded) return BadRequest(result.Errors);

        var starterQuotes = new[]
        {
            new Quote { Text = "Not all those who wander are lost.", Author = "J.R.R. Tolkien", UserId = user.Id },
            new Quote { Text = "It always seems impossible until it's done.", Author = "Nelson Mandela", UserId = user.Id },
            new Quote { Text = "The only way out is through.", Author = "Robert Frost", UserId = user.Id },
            new Quote { Text = "Stay hungry, stay foolish.", Author = "Steve Jobs", UserId = user.Id },
            new Quote { Text = "The journey of a thousand miles begins with a single step.", Author = "Lao Tzu", UserId = user.Id }
        };
        _context.Quotes.AddRange(starterQuotes);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Registreringen lyckades!" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            return Unauthorized(new { message = "Fel e-post eller lösenord." });

        await CreateRefreshSession(user.Id);
        return Ok(new { token = CreateAccessToken(user), email = user.Email });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        if (!Request.Cookies.TryGetValue(CookieName, out var rawToken) || string.IsNullOrWhiteSpace(rawToken))
            return Unauthorized();

        var hash = HashToken(rawToken);
        var session = await _context.RefreshSessions.FirstOrDefaultAsync(s => s.TokenHash == hash);
        if (session == null || session.RevokedAtUtc != null || session.ExpiresAtUtc <= DateTime.UtcNow)
        {
            ClearCookie();
            return Unauthorized();
        }

        var user = await _userManager.FindByIdAsync(session.UserId);
        if (user == null)
        {
            ClearCookie();
            return Unauthorized();
        }

        session.RevokedAtUtc = DateTime.UtcNow;
        await CreateRefreshSession(user.Id);
        return Ok(new { token = CreateAccessToken(user), email = user.Email });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        if (Request.Cookies.TryGetValue(CookieName, out var rawToken) && !string.IsNullOrWhiteSpace(rawToken))
        {
            var hash = HashToken(rawToken);
            var session = await _context.RefreshSessions.FirstOrDefaultAsync(s => s.TokenHash == hash);
            if (session != null) session.RevokedAtUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
        ClearCookie();
        return Ok(new { message = "Utloggad." });
    }

    private async Task CreateRefreshSession(string userId)
    {
        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var expires = DateTime.UtcNow.AddDays(30);
        _context.RefreshSessions.Add(new RefreshSession
        {
            UserId = userId,
            TokenHash = HashToken(rawToken),
            ExpiresAtUtc = expires
        });
        await _context.SaveChangesAsync();
        Response.Cookies.Append(CookieName, rawToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = !HttpContext.Request.IsHttps ? false : true,
            SameSite = SameSiteMode.Lax,
            Path = "/api/auth",
            Expires = new DateTimeOffset(expires)
        });
    }

    private void ClearCookie() => Response.Cookies.Delete(CookieName, new CookieOptions
    {
        Path = "/api/auth",
        SameSite = SameSiteMode.Lax,
        Secure = HttpContext.Request.IsHttps
    });

    private static string HashToken(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    private string CreateAccessToken(IdentityUser user)
    {
        var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT-nyckel saknas.");
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: credentials
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
