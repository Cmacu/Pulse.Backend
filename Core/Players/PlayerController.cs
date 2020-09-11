using System.Collections.Generic;
using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pulse.Core.PlayerSettings;

namespace Pulse.Core.Players {
    [Route("[controller]")]
    [ApiController]
    public class PlayerController : ControllerBase {
        private readonly PlayerService _playerService;
        private readonly PlayerSettingService _playerSettingService;

        private readonly IMapper _mapper;

        public PlayerController(PlayerService playerService, PlayerSettingService playerSettingService, IMapper mapper) {
            _playerService = playerService;
            _playerSettingService = playerSettingService;
            _mapper = mapper;
        }

        /// <summary>
        /// Retrieve the current player's private profile.
        /// </summary>
        [HttpGet]
        [Authorize]
        [Route("")]
        public ActionResult<PlayerModel> Get() {
            var playerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var model = _playerService.Get(playerId);

            if (model == null)
                return Unauthorized("Player not found");
            model.Status = _playerService.GetStatus(playerId);
            return model;
        }

        /// <summary>
        /// Retrieve the current player's settings.
        /// </summary>
        [HttpGet]
        [Authorize]
        [Route("settings")]
        public ActionResult<PlayerSettingsModel> Settings() {
            var playerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            return _playerSettingService.Get(playerId);
        }

        /// <summary>
        /// Update current player's country
        /// </summary>
        /// <param name="country">The country that should be assigned.</param>
        /// <returns>Confirmation that the value was set.</returns>
        [HttpPost]
        [Authorize]
        [Route("country")]
        public ActionResult<string> SetCountry(string country) {
            var playerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            _playerService.SetCountry(playerId, country);
            return Ok();
        }

        /// <summary>
        /// Update current player's setting for email notifications
        /// </summary>
        /// <param name="notify">True if the player should receive email notifications, false otherwise.</param>
        /// <returns>Confirmation that the value was set.</returns>
        [HttpPost]
        [Authorize]
        [Route("emails")]
        public ActionResult<string> SetEmailNotifications(bool notify) {
            var playerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            _playerSettingService.Set(playerId, "EmailNotifications", notify.ToString());
            return Ok();
        }

        /// <summary>
        /// Retrieve the specified player.
        /// </summary>
        /// <param name="username">The username of the player to get.</param>
        [HttpGet]
        [Authorize]
        [Route("{username}")]
        public ActionResult<PlayerModel> Get(string username) {
            return _playerService.Get(username);
        }

        /// <summary>
        /// Set player avatar to gravatar or clear it.
        /// </summary>
        /// <param name="active">should the gravatar be active or not.</param>
        /// <returns>Confirmation when the avatar is updated.</returns>
        [HttpPost]
        [Authorize]
        [Route("gravatar")]
        public ActionResult<string> SetGravatar(bool active) {
            var playerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            _playerService.SetGravatar(playerId, active);
            return Ok();
        }

        [HttpGet]
        [Route("search")]
        public ActionResult<List<string>> Search(string query) {
            return _playerService.Search(query);
        }
    }
}