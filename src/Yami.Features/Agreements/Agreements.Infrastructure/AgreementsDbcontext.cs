using Microsoft.EntityFrameworkCore;

public class AgreementsDbContext : DbContext
{
    public AgreementsDbContext(DbContextOptions<AgreementsDbContext> options) : base(options) { }

    public DbSet<Agreement> Agreements { get; set; } // maps to the "Agreements" table
    public DbSet<Installment> Installments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Installment>()
            .HasOne<Agreement>()
            .WithMany()
            .HasForeignKey(installment => installment.agreementId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}