using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Pulse.Backend;
using Pulse.Core.Players;
using Pulse.Matchmaker.Matches;
using Pulse.Ranking.Decay;

namespace Pulse.Ranking.Rating {
    public class DivisionModel {
        public Division Division { get; set; }
        public int Level { get; set; }
    }

    public class PlayerRating {
        public double RatingMean { get; set; }
        public double RatingDeviation { get; set; }
        public int DecayDays { get; set; }
        public Division Division { get; set; }
        public int Level { get; set; }
        public DateTime LastMatchTimestamp { get; set; }
    }

    public class RatingService {
        private readonly DataContext _context;
        private readonly RatingConfig _ratingConfig;
        private readonly DecayService _decayService;

        public RatingService(DataContext context, DecayService decayService) {
            _ratingConfig = new RatingConfig();
            _context = context;
            _decayService = decayService;
        }

        public void RerateAll() {
            var players = _context.Players
                .Include(x => x.Matches)
                .ToList();

            // Reset player ratings to their initial values
            var ratings = new Dictionary<int, PlayerRating>();
            foreach (var p in players) {
                ratings.Add(p.Id, GetDefaultRating());
            }
            ratings[1442].Level = 1;

            var matches = _context.Matches
                .Include(x => x.MatchPlayers)
                .Where(x => x.Status != MatchStatus.InProgress)
                .OrderBy(x => x.StartDate)
                .ToList();

            foreach (var match in matches) {
                foreach (var matchPlayer in match.MatchPlayers) {
                    var playerRating = ratings.GetValueOrDefault(matchPlayer.PlayerId, GetDefaultRating());
                    var currentDecay = _decayService.GetDecaySteps(playerRating.DecayDays, playerRating.LastMatchTimestamp, match.StartDate);

                    matchPlayer.Player.Division = playerRating.Division;
                    matchPlayer.Player.Level = playerRating.Level;
                    matchPlayer.OldRatingMean = playerRating.RatingMean;
                    matchPlayer.OldRatingDeviation = playerRating.RatingDeviation;
                    matchPlayer.DecayDays = currentDecay;
                }
                RateMatch(match);
                foreach (var matchPlayer in match.MatchPlayers) {
                    var playerRating = ratings.GetValueOrDefault(matchPlayer.PlayerId, GetDefaultRating());
                    var newDivision = GetNewDivisionAndLevel(playerRating.Division, playerRating.Level, matchPlayer.IsWin);
                    playerRating.Division = newDivision.Division;
                    playerRating.Level = newDivision.Level;
                    playerRating.RatingMean = matchPlayer.NewRatingMean;
                    playerRating.RatingDeviation = matchPlayer.NewRatingDeviation;
                    playerRating.DecayDays = matchPlayer.DecayDays;
                    playerRating.LastMatchTimestamp = match.StartDate;
                }
                if (match.Id % 10 == 0) _context.SaveChanges();
            }

            foreach (var p in players) {
                p.RatingMean = ratings[p.Id].RatingMean;
                p.RatingDeviation = ratings[p.Id].RatingDeviation;
                p.Division = ratings[p.Id].Division;
                p.Level = ratings[p.Id].Level;
            }

            _context.SaveChanges();
        }

        public void RateMatch(Match match) {
            var gameInfo = new Moserware.Skills.GameInfo(
                _ratingConfig.InitialMean,
                _ratingConfig.InitialDeviation,
                _ratingConfig.InitialMean / _ratingConfig.BetaDivisor,
                _ratingConfig.InitialMean / _ratingConfig.DynamicsFactorDivisor,
                _ratingConfig.DrawProbability
            );

            var teams = new List<Dictionary<Moserware.Skills.Player, Moserware.Skills.Rating>>();
            var players = new Dictionary<MatchPlayer, Moserware.Skills.Player>();

            foreach (var matchPlayer in match.MatchPlayers.OrderByDescending(x => x.IsWin)) {
                var trueSkillPlayer = new Moserware.Skills.Player(matchPlayer.PlayerId);
                var rating = new Moserware.Skills.Rating(matchPlayer.OldRatingMean, matchPlayer.OldRatingDeviation);
                players.Add(matchPlayer, trueSkillPlayer);

                var team = new Dictionary<Moserware.Skills.Player, Moserware.Skills.Rating>();
                team.Add(trueSkillPlayer, rating);
                teams.Add(team);
            }

            // Calculate the new ratings for each player, listing the winner first
            var newRatings = Moserware.Skills.TrueSkillCalculator.CalculateNewRatings(gameInfo, teams, 1, 2);

            // Save the new ratings back to the player entity
            foreach (var matchPlayer in match.MatchPlayers) {
                var trueSkillPlayer = players[matchPlayer];
                var newRating = newRatings[trueSkillPlayer];
                matchPlayer.NewRatingMean = GetMean(matchPlayer.OldRatingMean, newRating.Mean);
                matchPlayer.NewRatingDeviation = GetDeviation(matchPlayer.OldRatingDeviation, newRating.StandardDeviation);
                if (matchPlayer.IsWin) {
                    matchPlayer.DecayValue = _decayService.GetDecayValue(matchPlayer.DecayDays);
                    matchPlayer.DecayDays = Math.Max(0, matchPlayer.DecayDays - 1);
                }
                matchPlayer.RatingDelta = GetDelta(matchPlayer);
            }
        }

        public DivisionModel GetNewDivisionAndLevel(Division division, int level, bool isWin) {
            var player = new DivisionModel { Division = division, Level = level };

            if (player.Division == Division.Master) return player;

            if (player.Division == Division.Bronze) {
                if (isWin)
                    player.Level += 2;
                else
                    player.Level += 1;
            } else if (player.Division == Division.Silver) {
                if (isWin)
                    player.Level += 1;
                else
                    player.Level += 0;
            } else if (player.Division == Division.Gold) {
                if (isWin)
                    player.Level += 1;
                else
                    player.Level += -1;
            }

            if (player.Level > 3) {
                player.Level = 0;
                player.Division += 1;
            } else if (player.Level < 0)
                player.Level = 0;

            return player;
        }

        public void Reset(Player player) {
            player.RatingMean = _ratingConfig.InitialMean;
            player.RatingDeviation = _ratingConfig.InitialDeviation;
        }

        public double GetConservative(double ratingMean, double ratingDeviation) {
            return _ratingConfig.Offset + (_ratingConfig.Multiplier * (ratingMean - (ratingDeviation * _ratingConfig.ConservativeMultiplier)));
        }

        private double GetMean(double oldMean, double newMean) {
            var delta = newMean - oldMean;
            if (delta == 0) return newMean;

            var deltaAbs = Math.Abs(delta);
            var deltaSign = Math.Sign(delta);

            if (deltaAbs < _ratingConfig.MinDelta) return oldMean + _ratingConfig.MinDelta * deltaSign;
            if (deltaAbs > _ratingConfig.MaxDelta) return oldMean + _ratingConfig.MaxDelta * deltaSign;

            return newMean;
        }

        private double GetDeviation(double oldDeviation, double newDeviation) {
            if (newDeviation > oldDeviation) return oldDeviation;
            if (newDeviation < _ratingConfig.MinDeviation) return _ratingConfig.MinDeviation;
            return newDeviation;
        }

        private double GetDelta(MatchPlayer matchPlayer) {
            if (matchPlayer.Player.Division != Division.Master) return 0;
            var oldConservativeRating = GetConservative(matchPlayer.OldRatingMean, matchPlayer.OldRatingDeviation);
            var newConservativeRating = GetConservative(matchPlayer.NewRatingMean, matchPlayer.NewRatingDeviation);
            return newConservativeRating - oldConservativeRating;
        }

        private PlayerRating GetDefaultRating() {
            return new PlayerRating {
                RatingMean = _ratingConfig.InitialMean,
                    RatingDeviation = _ratingConfig.InitialDeviation,
                    Division = Division.Bronze,
                    Level = 0,
                    DecayDays = 0
            };
        }

    }
}