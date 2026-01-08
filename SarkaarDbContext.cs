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

    public DbSet<ImposterGame> ImposterGames { get; set; }
    public DbSet<ImposterPlayer> ImposterPlayers { get; set; }
    public DbSet<ImposterClue> ImposterClues { get; set; }
    public DbSet<ImposterVote> ImposterVotes { get; set; }
    public DbSet<ImposterRoundDecision> ImposterRoundDecisions { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Role>().HasData(
                    new Role { RoleId = 1, Name = "Admin" },
                    new Role { RoleId = 2, Name = "Viewer" },
                    new Role { RoleId = 3, Name = "TeamLead" }
        );
        // ImposterGame/ImposterPlayer relationship
        modelBuilder.Entity<ImposterPlayer>()
            .HasOne(p => p.Game)
            .WithMany(g => g.Players)
            .HasForeignKey(p => p.GameId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}