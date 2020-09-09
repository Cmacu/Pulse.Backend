using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Pulse.Configuration;
using Pulse.Core.Entities;
using Pulse.Matchmaker.Entities;
using Pulse.Matchmaker.Models;

namespace Pulse.Matchmaker.Services {
    public class MatchmakerLogService {
        private readonly int _aggregateWeeks = 4;
        private readonly DataContext _context;

        public MatchmakerLogService(DataContext context) {
            this._context = context;
        }

        /// <summary>
        /// Shows all active players from the matchmaker log
        /// </summary>
        /// <returns>List of players and their details</returns>
        public List<MatchmakerLog> ListLogs() {
            return this._context.MatchmakerLog.Where(x => x.ExpiredAt == null).ToList();
        }

        /// <summary>
        /// Lists player counts per hour in given timeframe
        /// </summary>
        /// <param name="from">Optional UTC DateTime start.</param>
        /// <param name="to">Optional UTC DateTime end.</param>
        /// <returns>List of players counts per hour</returns>
        public List<int> ListPlayerCounts(DateTime fromDate, DateTime toDate) {
            return this.ListLogCounters(fromDate, toDate).Select(x => x.PlayerCount).ToList();
        }

        /// <summary>
        /// Lists match counts per hour in given timeframe
        /// </summary>
        /// <param name="from">Optional UTC DateTime start.</param>
        /// <param name="to">Optional UTC DateTime end.</param>
        /// <returns>List of match counts per hour</returns>
        public List<int> ListMatchCounts(DateTime fromDate, DateTime toDate) {
            return this.ListLogCounters(fromDate, toDate).Select(x => x.MatchCount).ToList();
        }

        /// <summary>
        /// Lists average wait per hour in given timeframe
        /// </summary>
        /// <param name="from">Optional UTC DateTime start.</param>
        /// <param name="to">Optional UTC DateTime end.</param>
        /// <returns>List of average wait time (seconds) per hour</returns>
        public List<int> ListAverageWaitSeconds(DateTime fromDate, DateTime toDate) {
            return this.ListLogCounters(fromDate, toDate).Select(x => Convert.ToInt32(x.LogCount == 0 ? 0 : x.WaitSeconds / x.LogCount)).ToList();
        }

        /// <summary>
        /// Lists aggregate player counts per hour in given timeframe
        /// </summary>
        /// <param name="from">Optional UTC DateTime start.</param>
        /// <param name="to">Optional UTC DateTime end.</param>
        /// <returns>List of aggregate players counts per hour</returns>
        public List<int> ListPlayerAggregates(DateTime fromDate, DateTime toDate) {
            return this.ListLogAggregates(fromDate, toDate).Select(x => x.PlayerCount).ToList();
        }

        /// <summary>
        /// Lists aggregate match counts per hour in given timeframe
        /// </summary>
        /// <param name="from">Optional UTC DateTime start.</param>
        /// <param name="to">Optional UTC DateTime end.</param>
        /// <returns>List of aggregate matches counts per hour</returns>
        public List<int> ListMatchAggregates(DateTime fromDate, DateTime toDate) {
            return this.ListLogAggregates(fromDate, toDate).Select(x => x.MatchCount).ToList();
        }

        /// <summary>
        /// Lists aggregate wait per hour in given timeframe
        /// </summary>
        /// <param name="from">Optional UTC DateTime start.</param>
        /// <param name="to">Optional UTC DateTime end.</param>
        /// <returns>List of aggregate wait time (seconds) per hour</returns>
        public List<int> ListAggregateWaitSeconds(DateTime fromDate, DateTime toDate) {
            return this.ListLogAggregates(fromDate, toDate).Select(x => Convert.ToInt32(x.LogCount == 0 ? 0 : x.WaitSeconds / x.LogCount)).ToList();
        }

        /// <summary>
        /// Adds current player to the MatchmakerLog records
        /// </summary>
        /// <param name="playerDetails">The player details used by the matchmaker.</param>
        /// <returns></returns>
        public void Add(SeekModel playerDetails) {
            var matchmakerLog = new MatchmakerLog {
                PlayerId = int.Parse(playerDetails.Player),
                AddedAt = playerDetails.JoinedAt,
                Rating = playerDetails.Rating,
                RecentOpponents = string.Join(",", playerDetails.RecentOpponents)
            };
            this._context.Add<MatchmakerLog>(matchmakerLog);
            this._context.SaveChanges();
        }

        /// <summary>
        /// Marks the current player matchmaker logs as expired
        /// </summary>
        /// <param name="playerId">The current playerId to be marked as expired.</param>
        /// <returns></returns>
        public MatchmakerLog Expire(int playerId) {
            var rows = _context.MatchmakerLog
                .Where(x => x.PlayerId == playerId && x.ExpiredAt == null)
                .ToList();

            rows.ForEach(x => x.ExpiredAt = DateTime.UtcNow);
            _context.SaveChanges();

            return rows.FirstOrDefault();
        }

        /// <summary>
        /// Adds matchId to the matchmakerLog for the given list of players
        /// </summary>
        /// <param name="players">The players for which to set the matchId.</param>
        /// <param name="matchId">The matchId to be set.</param>
        /// <returns></returns>
        public void SetMatchId(IReadOnlyList<int> players, int matchId) {
            this._context.MatchmakerLog
                .Where(x => players.Contains(x.PlayerId) && x.ExpiredAt == null)
                .ToList()
                .ForEach(x => x.MatchId = matchId);
            this._context.SaveChanges();
        }

        private List<MatchmakerLogCounter> ListLogCounters(DateTime fromDate, DateTime toDate) {
            fromDate = RoundDown(fromDate, TimeSpan.FromHours(1));
            toDate = RoundDown(toDate, TimeSpan.FromHours(1));
            var logCounters = this.GetLogCounters(fromDate, toDate);
            var lastLogCountersDate = logCounters.Count() > 0 ? logCounters.Last().To : fromDate;
            if (lastLogCountersDate < toDate) {
                this.GenerateLogCounters(lastLogCountersDate, toDate);
                logCounters = this.GetLogCounters(fromDate, toDate);
            }

            return logCounters;
        }

        /// <summary>
        /// Gets LogAggregate
        /// </summary>
        private List<MatchmakerLogAggregate> ListLogAggregates(DateTime fromDate, DateTime toDate) {
            fromDate = RoundDown(fromDate, TimeSpan.FromHours(1));
            toDate = RoundDown(toDate, TimeSpan.FromHours(1));
            var logAggregates = this.GetLogAggregates(fromDate, toDate);
            var lastLogAggregateDate = logAggregates.Count() > 0 ? logAggregates.Last().To : fromDate;
            if (lastLogAggregateDate < toDate) {
                this.GenerateAggregates(lastLogAggregateDate, toDate);
                logAggregates = this.GetLogAggregates(fromDate, toDate);
            }

            return logAggregates;
        }

        private List<MatchmakerLogCounter> GetLogCounters(DateTime fromDate, DateTime toDate) {
            return this._context.MatchmakerLogCounter
                .Where(x => x.From >= fromDate && x.To <= toDate)
                .OrderBy(x => x.From).ToList();
        }

        private List<MatchmakerLogAggregate> GetLogAggregates(DateTime fromDate, DateTime toDate) {
            return this._context.MatchmakerLogAggregate
                .Where(x => x.From >= fromDate && x.To <= toDate)
                .OrderBy(x => x.From).ToList();
        }

        /// <summary>
        /// Generates LogCounts from MatchmakerLog.
        /// The function counts unique players, matches, number of logs and average waitSeconds
        /// based on the number of LogCounters count
        /// </summary>
        private void GenerateLogCounters(DateTime from, DateTime? until = null) {
            until = until ?? RoundDown(DateTime.UtcNow, TimeSpan.FromHours(1));
            var logs = this._context.MatchmakerLog.Where(x => x.AddedAt >= from && x.AddedAt <= until);
            var logEnumerator = logs.GetEnumerator();
            logEnumerator.MoveNext();
            var x = logEnumerator.Current;
            var logCounters = new List<MatchmakerLogCounter>();
            while (from < until) {
                var to = from.AddHours(1);
                var players = new Dictionary<int, bool>();
                var matches = new Dictionary<int, bool>();
                var waitSeconds = 0;
                var logCount = 0;
                while (x != null && x.AddedAt < to) {
                    waitSeconds += ((x.ExpiredAt ?? x.AddedAt.AddMinutes(5)) - x.AddedAt).Seconds;
                    players[x.PlayerId] = true;
                    matches[x.MatchId ?? 0] = true;
                    logCount++;
                    logEnumerator.MoveNext();
                    x = logEnumerator.Current;
                }
                logCounters.Add(new MatchmakerLogCounter {
                    From = from,
                        To = to,
                        PlayerCount = players.Count,
                        MatchCount = matches.Count,
                        LogCount = logCount,
                        WaitSeconds = waitSeconds,
                        CreatedAt = DateTime.UtcNow
                });
                from = to; // Don;t remove this, it will result in infinate loop
            }
            logEnumerator.Dispose();
            this._context.MatchmakerLogCounter.AddRange(logCounters);
            try {
                this._context.SaveChanges();
            } catch (DbUpdateException ex) {
                HandleException(ex);
            }
        }

        /// <summary>
        /// Generates Aggregate counts from the LogCounters
        /// by getting the previous N weeks from the MatchmakerLogCounter table
        /// and averaging players, matches, logs and waitSeconds
        /// based on the number of LogCounters count
        /// </summary>
        private void GenerateAggregates(DateTime from, DateTime? until = null) {
            until = until ?? RoundDown(DateTime.UtcNow, TimeSpan.FromHours(1));
            var logAggregates = new List<MatchmakerLogAggregate>();
            while (from < until) {
                var week1 = from.AddDays(-7 * 0);
                var week2 = from.AddDays(-7 * 1);
                var week3 = from.AddDays(-7 * 2);
                var week4 = from.AddDays(-7 * 3);
                var logCounters = this._context.MatchmakerLogCounter.Where(x =>
                    x.From == week1 ||
                    x.From == week2 ||
                    x.From == week3 ||
                    x.From == week4
                ).ToList();
                int players = 0;
                int matches = 0;
                int logs = 0;
                double waitSeconds = 0;
                var counter = 0;
                logCounters.ForEach(x => {
                    players += x.PlayerCount;
                    matches += x.MatchCount;
                    waitSeconds += x.WaitSeconds;
                    logs += x.LogCount;
                    counter++;
                });
                if (counter == 0) counter = this._aggregateWeeks; // Avoid division by 0
                var to = from.AddHours(1);
                logAggregates.Add(new MatchmakerLogAggregate {
                    From = from,
                        To = to,
                        PlayerCount = players / counter,
                        MatchCount = matches / counter,
                        LogCount = logs / counter,
                        WaitSeconds = waitSeconds / counter,
                        CreatedAt = DateTime.UtcNow
                });
                from = to; // Don;t remove this, it will result in infinate loop
            }
            this._context.MatchmakerLogAggregate.AddRange(logAggregates);
            try {
                this._context.SaveChanges();
            } catch (DbUpdateException ex) {
                HandleException(ex);
            }
        }

        private void HandleException(Exception exception) {
            if (!exception.Message.Contains("UniqueConstraint")) {
                throw exception;
            }
        }

        private DateTime RoundDown(DateTime dateTime, TimeSpan interval) {
            return dateTime.AddTicks(-(dateTime.Ticks % interval.Ticks));
        }
    }
}