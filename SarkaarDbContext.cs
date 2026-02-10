using Microsoft.EntityFrameworkCore;
using Sarkaar_Apis.Models;
using SarkaarGame.Models;

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
    public DbSet<ChatMessage> ChatMessages { get; set; }

    public DbSet<Party> Parties { get; set; }
    public DbSet<Player> Players { get; set; }

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

        // Define primary key for Party
        modelBuilder.Entity<Party>()
            .HasKey(p => p.PartyId);

        // Define primary key for Player
        modelBuilder.Entity<Player>()
            .HasKey(p => p.PlayerId);

        // Define foreign key relationship between Player and Party
        modelBuilder.Entity<Player>()
            .HasOne(p => p.Party)
            .WithMany(p => p.Players)
            .HasForeignKey(p => p.PartyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}