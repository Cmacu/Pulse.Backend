using System;
using System.Collections.Generic;
using System.Linq;
using Pulse.Configuration;

namespace Pulse.Rank.Services {
    public class DecayService {
        private readonly ApiConfiguration _configuration;
        private readonly int _decayStepsCount;
        private readonly int[] _decayAmounts;
        private readonly DateTime _decayStartAt;
        private readonly DateTime _decayEndAt;

        public DecayService(ApiConfiguration configuration) {
            _configuration = configuration;

            _decayStepsCount = _configuration.Decay.TotalSteps;
            _decayStartAt = _configuration.Decay.StartAt;
            _decayEndAt = _configuration.Decay.EndAt;
            _decayAmounts = _configuration.Decay.StepAmounts;
        }

        /// <summary>
        /// Adds the number of steps that had already accrued prior to the given time range, to a
        /// calculation of the number of decay steps that have accrued during the given time range.
        /// </summary>
        /// <param name="prevSteps">The number of steps that had already accrued prior to the given time range.</param>
        /// <param name="prevTimestamp">The start of the given time range. If not provided, the given range is assumed to be zero hours.</param>
        /// <param name="currentTimestamp">The end of the given time range. If not provided, the current time is used.</param>
        /// <returns>The total number of decay steps from all sources.</returns>
        public int GetDecaySteps(int prevSteps, DateTime? prevTimestamp, DateTime? currentTimestamp = null) {
            if (prevTimestamp < _decayStartAt) {
                prevTimestamp = _decayStartAt;
            }
            var nextTimestamp = currentTimestamp ?? DateTime.UtcNow;
            if (nextTimestamp > _decayEndAt) {
                nextTimestamp = _decayEndAt;
            }
            var addSteps = GetDecayStepsInTimeRange(nextTimestamp, prevTimestamp);
            return AddDecaySteps(prevSteps, addSteps);
        }

        /// <summary>
        /// Sums the total amount of rating decay that results from
        /// the number of steps that had already accrued prior to the given time range, and a
        /// calculation of the number of decay steps that have accrued during the given time range.
        /// </summary>
        /// <param name="prevSteps">The number of steps that had already accrued prior to the given time range.</param>
        /// <param name="prevTimestamp">The start of the given time range. If not provided, the given range is assumed to be zero hours.</param>
        /// <param name="currentTimestamp">The end of the given time range. If not provided, the current time is used.</param>
        /// <returns>The total amount of rating decay from all sources.</returns>
        public int GetDecayValues(int prevSteps, DateTime? prevTimestamp = null, DateTime? currentTimestamp = null) {
            var currentDecay = GetDecaySteps(prevSteps, prevTimestamp, currentTimestamp);
            var totalDecay = 0;
            for (int i = 0; i < Math.Min(currentDecay, _decayStepsCount); i++) {
                totalDecay += _decayAmounts[i];
            }

            return totalDecay;
        }

        /// <summary>
        /// Retrieves the rating decay value that would be recovered by a single decay step.
        /// </summary>
        /// <param name="decayStep">The given decay step.</param>
        public int GetDecayValue(int decayStep) {
            if (decayStep <= 0) return 0;
            if (decayStep > _decayAmounts.Count()) return _decayAmounts.Last();

            return _decayAmounts[decayStep - 1];
        }

        /// <summary>
        /// Adds two decay steps, limiting the result to the total possible number of decay steps.
        /// </summary>
        /// /// <param name="days">The first decay step addend.</param>
        /// <param name="additional">The second decay step addend.</param>
        private int AddDecaySteps(int days, int additional) {
            var total = days + additional;
            return Math.Min(total, _decayStepsCount);
        }

        /// <summary>
        /// Calculates the number of decay steps that accrued during the given time range.
        /// The first decay step is accrued after 48 hours; each subsequent step after 24 additional hours.
        /// </summary>
        /// <param name="currentTimestamp">The end of the given time range.</param>
        /// <param name="prevTimestamp">The start of the given time range. If not provided, the given time range is assumed to be zero hours.</param>
        private int GetDecayStepsInTimeRange(DateTime nextTimestamp, DateTime? prevTimestamp = null) {
            if (prevTimestamp == null) {
                return 0;
            }
            var days = (nextTimestamp - prevTimestamp).Value.Days;
            if (days <= 0) {
                return 0;
            }
            return Math.Min(days - 1, _decayStepsCount);
        }
    }
}