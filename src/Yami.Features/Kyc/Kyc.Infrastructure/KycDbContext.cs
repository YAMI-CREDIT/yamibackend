using Microsoft.EntityFrameworkCore;

// The bridge between C# and the Kyc tables in the database.
// Owns the verification process records.
public class KycDbContext : DbContext
{
    public KycDbContext(DbContextOptions<KycDbContext> options) : base(options) { }

    public DbSet<KycVerification> KycVerifications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Store the enum as a readable string ("InProgress") instead of an int.
        modelBuilder.Entity<KycVerification>()
            .Property(k => k.status)
            .HasConversion<string>();

        // At most ONE inflight verification per entity. Terminal rows
        // (Verified/Failed/Error) are unlimited, so the audit trail is kept.
        modelBuilder.Entity<KycVerification>()
            .HasIndex(k => k.entityId)
            .IsUnique()
            .HasFilter("\"status\" = 'InProgress'");
    }
}
