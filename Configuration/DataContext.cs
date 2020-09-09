using System.Linq;
using Microsoft.EntityFrameworkCore;
using Pulse.Core.Entities;
using Pulse.Matchmaker.Entities;
using Pulse.Rank.Entities;

namespace Pulse.Configuration {

    public class DataContext : DbContext {
        public DataContext(DbContextOptions<DataContext> options) : base(options) {}

        public DbSet<EmailLog> EmailLog { get; set; }
        public DbSet<AppError> AppError { get; set; }

        public DbSet<Match> Match { get; set; }
        public DbSet<MatchPlayer> MatchPlayer { get; set; }
        public DbSet<MatchmakerLog> MatchmakerLog { get; set; }
        public DbSet<MatchmakerLogCounter> MatchmakerLogCounter { get; set; }
        public DbSet<MatchmakerLogAggregate> MatchmakerLogAggregate { get; set; }

        public DbSet<Player> Player { get; set; }
        public DbSet<PlayerBadge> PlayerBadge { get; set; }
        public DbSet<PlayerSession> PlayerSession { get; set; }
        public DbSet<PlayerSetting> PlayerSetting { get; set; }
        public DbSet<LeaderboardLog> LeaderboardLog { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            // Set max length to 256 by default so string can be created as varchar
            var stringProperties = modelBuilder.Model
                .GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(string));
            foreach (var property in stringProperties) {
                if (property.GetMaxLength() == null) property.SetMaxLength(256);
            }

            // Add unique indexes to avoid duplicate dates
            modelBuilder.Entity<Player>().HasIndex(x => x.Email).IsUnique();

            modelBuilder.Entity<MatchmakerLogCounter>().HasIndex(x => x.From).IsUnique();
            modelBuilder.Entity<MatchmakerLogAggregate>().HasIndex(x => x.From).IsUnique();

            modelBuilder.Entity<LeaderboardLog>().HasIndex(x => x.CreatedAt);
            modelBuilder.Entity<LeaderboardLog>().HasIndex(x => new { x.CreatedAt, x.PlayerId }).IsUnique();

            base.OnModelCreating(modelBuilder);
        }
    }
}