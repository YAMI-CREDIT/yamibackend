using Microsoft.EntityFrameworkCore;

// This is the bridge between C# and the actual database (EF Core's Object relational mapper)
public class UsersDbContext : DbContext
{
    public UsersDbContext(DbContextOptions<UsersDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; } // maps to the "Users" table
    public DbSet<Invitation> Invitations { get; set; } // maps to the "Invitations" table

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasIndex(user => user.phone)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(user => user.userSubId)
            .IsUnique();
    }
}