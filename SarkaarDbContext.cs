using Microsoft.EntityFrameworkCore;
public class SarkaarDbContext : DbContext
{
    public SarkaarDbContext(DbContextOptions<SarkaarDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Team> Teams { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Role>().HasData(
                    new Role { RoleId = 1, Name = "Admin" },
                    new Role { RoleId = 2, Name = "Viewer" },
                    new Role { RoleId = 3, Name = "TeamLead" }
        );

        modelBuilder.Entity<Team>()
            .HasOne(t=> t.TeamLead)
            .WithMany()
            .HasForeignKey(t => t.TeamLeadId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}