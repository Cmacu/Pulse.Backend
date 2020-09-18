using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Pulse.Backend;
using Pulse.Matchmaker.Matches;
using Pulse.Ranking.Decay;
using Pulse.Ranking.Rating;

namespace Pulse.Ranking.Leaderboard {
  public class LeaderboardService {
    private readonly DataContext _context;
    private readonly MatchService _matchService;
    private readonly RatingService _ratingService;
    private readonly DecayService _decayService;
    private readonly IMemoryCache _cache;
    private readonly string _leaderboardKey;
    private readonly DateTime _seasonStart;

    public LeaderboardService(
      DataContext context,
      MatchService matchService,
      RatingService ratingService,
      DecayService decayService,
      IMemoryCache cache
    ) {
      _context = context;
      _matchService = matchService;
      _ratingService = ratingService;
      _decayService = decayService;
      _cache = cache;
      var _leaderboardConfig = new LeaderboardConfig();
      _leaderboardKey = _leaderboardConfig.CacheKey;
      _seasonStart = _leaderboardConfig.SeasonStart;
    }

    public List<LeaderboardLog> GetLog(string username) {
      return _context.LeaderboardLogs
        .Include(x => x.Player)
        .Where(x => (username == null || x.Player.Username == username) &&
          (x.DeletedAt == null || x.DeletedAt > DateTime.UtcNow))
        .OrderBy(x => x.CreatedAt)
        .ToList();
    }

    public PagedLeaderboardResponse Get(int skip, int take) {
      var leaderboard = GetLeaderboard();

      var rows = leaderboard
        .Skip(skip)
        .Take(take)
        .ToList();

      var response = new PagedLeaderboardResponse() {
        Total = leaderboard.Count,
        Results = rows,
        CreatedAt = DateTime.UtcNow
      };

      if (leaderboard.Count == 0) return response;
      response.CreatedAt = rows.Last().CreatedAt;

      return response;
    }

    public void RemoveCache() {
      _cache.Remove(_leaderboardKey);
    }

    public void Truncate() {
      _context.Database.ExecuteSqlRaw("DELETE from LeaderboardLog");
    }

    private List<LeaderboardResponse> GetLeaderboard() {
      DateTime lastGeneratedAt;
      List<LeaderboardResponse> leaderboard;

      if (!_cache.TryGetValue(_leaderboardKey, out leaderboard))
        lastGeneratedAt = GetLastGeneratedAt();
      else
        lastGeneratedAt = leaderboard.Count > 0 ? leaderboard.Last().CreatedAt : DateTime.UtcNow;

      var newGeneratedAt = GenerateLeaderboardLogs(lastGeneratedAt);

      if (leaderboard == null || lastGeneratedAt < newGeneratedAt) {
        leaderboard = LoadLeaderboard(newGeneratedAt);
        _cache.Set(_leaderboardKey, leaderboard);
      }

      return leaderboard;
    }

    private List<LeaderboardResponse> LoadLeaderboard(DateTime day) {
      return _context.LeaderboardLogs
        .Include(x => x.Player)
        .Where(x => x.CreatedAt == day &&
          (x.DeletedAt == null || x.DeletedAt > DateTime.UtcNow))
        .Select(x => new LeaderboardResponse() {
          PlayerId = x.PlayerId,
            Username = x.Player.Username,
            Avatar = x.Player.Avatar,
            Country = x.Player.Country,
            Rank = x.Rank,
            PreviousRank = x.PreviousRank,
            LeaderboardRating = x.ConservativeRating - x.TotalDecay,
            TotalDecay = x.TotalDecay,
            CreatedAt = x.CreatedAt,
        })
        .OrderByDescending(x => x.LeaderboardRating)
        .ToList();
    }

    private DateTime GenerateLeaderboardLogs(DateTime lastGeneratedAt) {
      _matchService.CompleteMatches();
      while ((DateTime.UtcNow - lastGeneratedAt).TotalHours > 24d) {
        lastGeneratedAt = lastGeneratedAt.AddHours(24);
        this.ComputeDay(lastGeneratedAt);
      }

      return lastGeneratedAt;
    }

    private DateTime GetLastGeneratedAt() {
      var lastGeneratedAt = _context.LeaderboardLogs.OrderByDescending(x => x.CreatedAt).Select(x => x.CreatedAt).FirstOrDefault();
      if (lastGeneratedAt == null || lastGeneratedAt < _seasonStart)
        lastGeneratedAt = _seasonStart;

      return lastGeneratedAt;
    }

    private void ComputeDay(DateTime day) {
      var rows = _context.Players
        .Include(x => x.Matches)
        .ThenInclude(x => x.Match)
        .Where(x => x.Division == Division.Master)
        .Select(x => new {
          LastMatch = x.Matches.Where(x => x.Match.StartDate < day).OrderByDescending(x => x.Match.StartDate).First(),
            Player = x
        })
        .Select(x => new LeaderboardLog() {
          CreatedAt = day,
            ConservativeRating = _ratingService.GetConservative(x.LastMatch.NewRatingMean, x.LastMatch.NewRatingDeviation),
            TotalDecay = _decayService.GetDecayValues(x.LastMatch.DecayDays, x.LastMatch.Match.StartDate, day),
            PreviousRank = x.Player.Level,
            PlayerId = x.Player.Id
        })
        .ToList()
        .OrderByDescending(x => x.ConservativeRating - x.TotalDecay)
        .ToList();

      for (int i = 0; i < rows.Count; i++) {
        rows[i].Rank = i + 1;
      }

      var players = _context.Players.Where(x => x.Division == Division.Master).ToList();

      foreach (var player in players) {
        player.Level = rows.Where(x => x.PlayerId == player.Id).Select(x => x.Rank).FirstOrDefault();
      }

      _context.LeaderboardLogs.AddRange(rows);
      _context.SaveChanges();
    }
  }
}