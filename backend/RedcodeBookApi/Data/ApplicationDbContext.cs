using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RedcodeBookApi.Models;

namespace RedcodeBookApi.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options
    ) : base(options)
    {
    }

    public DbSet<Book> Books => Set<Book>();

    public DbSet<Quote> Quotes => Set<Quote>();

    public DbSet<RefreshSession> RefreshSessions => Set<RefreshSession>();
}

