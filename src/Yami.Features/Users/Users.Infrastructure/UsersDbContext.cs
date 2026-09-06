using Microsoft.EntityFrameworkCore;

// This is the bridge between C# and the actual database (EF Core's Object relational mapper)
public class UsersDbContext : DbContext
{
    public UsersDbContext(DbContextOptions<UsersDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; } // maps to the "Users" table
    public DbSet<OtpCode> OtpCodes { get; set; }
    public DbSet<BusinessProfile> BusinessProfiles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.PhoneNumber).IsUnique();

            // Email and GoogleId are optional, but must be unique when present.
            entity.HasIndex(u => u.Email).IsUnique().HasFilter("\"Email\" IS NOT NULL");
            entity.HasIndex(u => u.GoogleId).IsUnique().HasFilter("\"GoogleId\" IS NOT NULL");

            entity.HasOne(u => u.BusinessProfile)
                .WithOne()
                .HasForeignKey<BusinessProfile>(bp => bp.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OtpCode>(entity =>
        {
            // Speeds up "find the newest OTP for this destination/purpose" lookups.
            entity.HasIndex(o => new { o.Destination, o.Purpose, o.CreatedAt });
        });

        modelBuilder.Entity<BusinessProfile>(entity =>
        {
            entity.HasIndex(bp => bp.UserId).IsUnique();
        });
    }
}
