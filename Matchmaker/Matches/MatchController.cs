using System.Collections.Generic;
using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pulse.Ranking.Rating;

namespace Pulse.Matchmaker.Matches {
    [Route("[controller]")]
    [ApiController]
    public class MatchController : ControllerBase {
        private readonly MatchService _matchService;
        private readonly RatingService _ratingService;
        private readonly IMapper _mapper;

        public MatchController(MatchService matchService, RatingService ratingService, IMapper mapper) {
            _matchService = matchService;
            _ratingService = ratingService;
            _mapper = mapper;
        }

        /// <summary>
        /// Retrieve the most recent matches.
        /// </summary>
        /// <param name="player">The optional username of the player. If provided, only matches played by the player will be returned.</param>
        /// <param name="opponent">The optional username of the opponent. If provided, only matches against the opponent will be returned.</param>
        /// <param name="skip">The number of matches to skip.</param>
        /// <param name="take">The number of matches to retrieve.</param>
        [HttpGet]
        [Authorize]
        [Route("")]
        public ActionResult<PagedMatchModel> Get(string player, string opponent, int skip, int take) {
            return _matchService.GetRecent(player, opponent, skip, take);
        }

        /// <summary>
        /// Retrieve the most recent (possibly unfinished) match of the current player.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        [Route("Last")]
        public ActionResult<MatchModel> Last() {
            var playerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var lastMatch = _matchService.GetLastMatchByPlayerId(playerId);

            if (lastMatch == null)
                return NotFound();
            var result = new ResultModel();
            return _matchService.UpdateMatch(lastMatch, result);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        [Route("Create")]
        public ActionResult<int> Create(List<int> playerIds) {
            return _matchService.CreateMatch(playerIds, 1).Id;
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        [Route("Update")]
        public ActionResult<MatchModel> Update(int matchId) {
            var match = _matchService.Find(matchId);
            var result = new ResultModel();
            return _matchService.UpdateMatch(match, result);
        }

        [HttpGet]
        // [Authorize(Roles = "Administrator")]
        [Route("Complete")]
        public ActionResult Complete() {
            _matchService.CompleteMatches();
            return Ok();
        }

        [HttpGet]
        // [Authorize(Roles = "Administrator")]
        [Route("rerate")]
        public ActionResult Rerate() {
            _ratingService.RerateAll();
            return Ok();
        }
    }
}