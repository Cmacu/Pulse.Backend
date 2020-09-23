using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Pulse.Backend;
using Pulse.Core.Notifications;
using Pulse.Core.Players;
using Pulse.Ranking.Decay;
using Pulse.Ranking.Rating;

namespace Pulse.Matchmaker.Matches {
  public class MatchService {
    private readonly NotificationService _notificationService;
    private readonly RatingService _ratingService;
    private readonly DecayService _decayService;
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public MatchService(
      NotificationService notificationService,
      RatingService ratingService,
      DecayService decayService,
      DataContext context,
      IMapper mapper
    ) {
      _notificationService = notificationService;
      _ratingService = ratingService;
      _decayService = decayService;
      _context = context;
      _mapper = mapper;
    }

    public Match Find(int matchId) {
      return _context.Matches
        .Include(x => x.MatchPlayers)
        .ThenInclude(x => x.Player)
        .FirstOrDefault(x => x.Id == matchId);
    }

    public MatchResponse UpdateMatch(Match match, ResultModel result) {
      // Retrieve data from the latest match information
      if (match.Status == MatchStatus.InProgress && match.UpdatedAt.AddSeconds(30) < DateTime.UtcNow) {
        match = UpdateMatchResult(match, result);
        match.UpdatedAt = DateTime.UtcNow;
        _context.SaveChanges();
      }

      return _mapper.Map<MatchResponse>(match);
    }

    /// <summary>
    /// Create a new match in the platform and in the database.
    /// </summary>
    /// <param name="playerIds">The IDs of the players in the match.</param>
    /// <returns>The database ID of the new match.</returns>
    public int CreateMatch(List<int> playerIds) {
      var name = GenerateMatchName();
      var match = new Match() {
        Name = name,
        StartDate = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
        MatchPlayers = new List<MatchPlayer>()
      };

      var players = _context.Players
        .Where(x => playerIds.Contains(x.Id))
        .ToList();

      foreach (var player in players) {
        match.MatchPlayers.Add(new MatchPlayer() {
          Player = player,
            PlayerId = player.Id,
            Position = playerIds.IndexOf(player.Id),
            OldRatingMean = player.RatingMean,
            OldRatingDeviation = player.RatingDeviation,
            NewRatingMean = player.RatingMean,
            NewRatingDeviation = player.RatingDeviation,
            DecayDays = GetPlayerDecay(player)
        });
      }

      _context.Add(match);
      _context.SaveChanges();

      return match.Id;
    }

    public Match GetLastMatchByPlayerId(int playerId) {
      return _context.Matches
        .Include(x => x.MatchPlayers)
        .ThenInclude(x => x.Player)
        .Where(x => x.MatchPlayers.Any(x => x.PlayerId == playerId))
        .OrderByDescending(x => x.StartDate)
        .FirstOrDefault();
    }

    public PagedMatchResponse GetRecent(int? playerId, int? opponentId, int skip, int take) {
      var q = _context.Matches
        .Include(x => x.MatchPlayers)
        .ThenInclude(x => x.Player)
        .Include(x => x.MatchPlayers)
        .Where(x => x.EndDate != null &&
          x.MatchPlayers.Any(x => playerId == null || x.PlayerId == playerId) &&
          x.MatchPlayers.Any(x => opponentId == null || x.PlayerId == opponentId));

      var total = q.Count();

      var match = q
        .OrderByDescending(x => x.StartDate)
        .Skip(skip)
        .Take(take)
        .ToList();

      var results = _mapper.Map<List<MatchResponse>>(match);

      return new PagedMatchResponse() {
        Total = total,
          Results = results
      };
    }
    public PagedMatchResponse GetRecent(string player, string opponent, int skip, int take) {
      var q = _context.Matches
        .Include(x => x.MatchPlayers)
        .ThenInclude(x => x.Player)
        .Include(x => x.MatchPlayers)
        .Where(x => x.EndDate != null &&
          x.MatchPlayers.Any(x => player == null || x.Player.Username == player) &&
          x.MatchPlayers.Any(x => opponent == null || x.Player.Username == opponent));

      var total = q.Count();

      var match = q
        .OrderByDescending(x => x.StartDate)
        .Skip(skip)
        .Take(take)
        .ToList();

      var results = _mapper.Map<List<MatchResponse>>(match);

      return new PagedMatchResponse() {
        Total = total,
          Results = results
      };
    }

    public void CompleteMatches() {
      var matches = _context.Matches
        .Include(x => x.MatchPlayers)
        .ThenInclude(x => x.Player)
        .Where(x => x.Status == MatchStatus.InProgress && x.StartDate < DateTime.UtcNow.AddHours(-2))
        .ToList();

      foreach (var match in matches) {
        try {
          var result = new ResultModel();
          UpdateMatch(match, result);
        } catch (Exception ex) {
          _notificationService.ExceptionCaught(ex);
        }
      }
    }

    private Match UpdateMatchResult(Match match, ResultModel result) {
      match.Status = result.Status;

      if (match.Status == MatchStatus.InProgress)
        return match;

      // Match is over. Handle match completion
      match.EndDate = DateTime.UtcNow;
      foreach (var matchPlayer in match.MatchPlayers) {
        UpdateMatchPlayer(matchPlayer, result);
      }

      _ratingService.RateMatch(match);

      foreach (var matchPlayer in match.MatchPlayers) {
        matchPlayer.Player.RatingMean = matchPlayer.NewRatingMean;
        matchPlayer.Player.RatingDeviation = matchPlayer.NewRatingDeviation;
      }

      return match;
    }

    private int GetPlayerDecay(Player player) {
      if (player.Division != Division.Master) return 0;
      var lastMatch = GetLastMatchByPlayerId(player.Id);
      if (lastMatch == null) return 0;
      var previousDecay = lastMatch.MatchPlayers.ToList().Where(x => x.PlayerId == player.Id).Select(x => x.DecayDays).FirstOrDefault();
      return _decayService.GetDecaySteps(previousDecay, lastMatch.StartDate);
    }

    private void UpdateMatchPlayer(MatchPlayer matchPlayer, ResultModel result) {
      var playerId = matchPlayer.Player.Id.ToString();
      var player = result.Players.First(x => x.PlayerId == playerId);
      matchPlayer.Score = player.Score;
      matchPlayer.Status = player.Status;

      // If the scores are tied, the player in the lower position loses the tiebreak
      matchPlayer.IsWin = ((double) matchPlayer.Score + ((double) matchPlayer.Position * 0.25)) > (double) result.Players.Where(x => x.PlayerId != playerId).Max(x => x.Score);

      var newDivision = _ratingService.GetNewDivisionAndLevel(matchPlayer.Player.Division, matchPlayer.Player.Level, matchPlayer.IsWin);
      matchPlayer.Player.Division = newDivision.Division;
      matchPlayer.Player.Level = newDivision.Level;
    }

    private string GenerateMatchName() {
      var r = new Random();
      return $"Pulse {r.Next(1000, 9999)}";
    }
  }
}