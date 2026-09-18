using Microsoft.EntityFrameworkCore;

public class AgreementsDbContext : DbContext
{
    public AgreementsDbContext(DbContextOptions<AgreementsDbContext> options) : base(options) { }

    public DbSet<Agreement> Agreements { get; set; } // maps to the "Agreements" table
}