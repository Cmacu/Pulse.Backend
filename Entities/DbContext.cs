using Microsoft.EntityFrameworkCore;
using Pulse.Entities.Core;
using Pulse.Entities.Match;
using Pulse.Entities.Player;

namespace Pulse.Entities
{

    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) {}

        public DbSet<EmailLog> EmailLogs { get; set; }
        public DbSet<AppError> AppErrors { get; set; }

        public DbSet<Match.Match> Matches { get; set; }
        public DbSet<MatchPlayer> MatchPlayers { get; set; }
        public DbSet<MatchmakerLog> MatchmakerLogs { get; set; }
        public DbSet<MatchmakerLogCounter> MatchmakerLogCounters { get; set; }
        public DbSet<MatchmakerLogAggregate> MatchmakerLogAggregates { get; set; }

        public DbSet<Player.Player> Players { get; set; }
        public DbSet<PlayerBadge> PlayerBadges { get; set; }
        public DbSet<PlayerSession> PlayerSessions { get; set; }
        public DbSet<PlayerSetting> PlayerSettings { get; set; }
        public DbSet<LeaderboardLog> LeaderboardLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Add unique indexes to avoid duplicate dates
            modelBuilder.Entity<MatchmakerLogCounter>().HasIndex(x => x.From).IsUnique();
            modelBuilder.Entity<MatchmakerLogAggregate>().HasIndex(x => x.From).IsUnique();

            modelBuilder.Entity<LeaderboardLog>().HasIndex(x => x.CreatedAt);
            modelBuilder.Entity<LeaderboardLog>().HasIndex(x => new { x.CreatedAt, x.PlayerId }).IsUnique();

            base.OnModelCreating(modelBuilder);
        }
    }
}