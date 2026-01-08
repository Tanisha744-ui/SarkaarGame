using Microsoft.EntityFrameworkCore;
using Sarkaar_Apis.Models;
public class SarkaarDbContext : DbContext
{
    public SarkaarDbContext(DbContextOptions<SarkaarDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Team> Teams { get; set; }
    public DbSet<Bid> Bids { get; set; }
    public DbSet<SarkaarGame.Models.GameControls> GameControls { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Role>().HasData(
                    new Role { RoleId = 1, Name = "Admin" },
                    new Role { RoleId = 2, Name = "Viewer" },
                    new Role { RoleId = 3, Name = "TeamLead" }
        );

        // TeamLead and TeamLeadId removed from Team
    }
}