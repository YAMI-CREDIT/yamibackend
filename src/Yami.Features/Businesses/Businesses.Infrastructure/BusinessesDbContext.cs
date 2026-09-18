using Microsoft.EntityFrameworkCore;
public class BusinessesDbContext : DbContext
{
    public BusinessesDbContext(DbContextOptions<BusinessesDbContext> options) : base(options) { }

    public DbSet<Business> Businesses { get; set; } // maps to the "Businesses" table
}