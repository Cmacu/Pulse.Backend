using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;
using Pulse.Core.AppErrors;
using Pulse.Core.Authorization;
using Pulse.Core.Notifications;
using Pulse.Core.Players;
using Pulse.Core.PlayerSettings;
using Pulse.Games.SchottenTotten2.Gameplay;
using Pulse.Games.SchottenTotten2.Persistance;
using Pulse.Matchmaker.Logs;
using Pulse.Matchmaker.Matches;
using Pulse.Ranking.Leaderboard;
using Pulse.Ranking.Rating;

namespace Pulse.Backend {

    public class DataContext : DbContext {
        public DataContext(DbContextOptions<DataContext> options) : base(options) {}

        public DbSet<EmailLog> EmailLogs { get; set; }
        public DbSet<AppError> AppErrors { get; set; }

        public DbSet<Match> Matches { get; set; }
        public DbSet<MatchPlayer> MatchPlayers { get; set; }
        public DbSet<MatchmakerLog> MatchmakerLogs { get; set; }
        public DbSet<MatchmakerLogCounter> MatchmakerLogCounters { get; set; }
        public DbSet<MatchmakerLogAggregate> MatchmakerLogAggregates { get; set; }

        public DbSet<Player> Players { get; set; }
        public DbSet<PlayerBadge> PlayerBadges { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<PlayerSetting> PlayerSettings { get; set; }
        public DbSet<LeaderboardLog> LeaderboardLogs { get; set; }
        public DbSet<Schotten2Game> Schotten2Games { get; set; }
        public DbSet<Schotten2Log> Schotten2Logs { get; set; }

        public class Schotten2GamesConfiguration : IEntityTypeConfiguration<Schotten2Game> {
            public void Configure(EntityTypeBuilder<Schotten2Game> builder) {
                // This Converter will perform the conversion to and from Json to the desired type
                builder.Property(e => e.State).HasConversion(
                    v => JsonConvert.SerializeObject(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }),
                    v => JsonConvert.DeserializeObject<GameState>(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
            }
        }
        public class Schotten2LogsConfiguration : IEntityTypeConfiguration<Schotten2Log> {
            public void Configure(EntityTypeBuilder<Schotten2Log> builder) {
                // This Converter will perform the conversion to and from Json to the desired type
                builder.Property(e => e.State).HasConversion(
                    v => JsonConvert.SerializeObject(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }),
                    v => JsonConvert.DeserializeObject<GameState>(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            // Set max length to 256 by default so string can be created as varchar
            var stringProperties = modelBuilder.Model
                .GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(string));
            foreach (var property in stringProperties) {
                if (property.GetMaxLength() == null) property.SetMaxLength(256);
            }

            modelBuilder.ApplyConfiguration(new Schotten2GamesConfiguration());
            modelBuilder.ApplyConfiguration(new Schotten2LogsConfiguration());

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