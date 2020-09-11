using System;
using Microsoft.AspNetCore.Mvc;

namespace Pulse.Core.Index {
    [ApiController]
    [Route("")]
    [Route("[controller]")]
    public class HeartbeatController : ControllerBase {

        public HeartbeatController() {}

        /// <summary>
        /// Confirm that the API is active and responding to requests.
        /// </summary>
        [HttpGet]
        [Route("")]
        public ActionResult Get() {
            return Ok(new Heartbeat() { Response = "OK" });
        }

        /// <summary>
        /// Throw an unhandled exception.
        /// </summary>
        [HttpGet]
        [Route("Error")]
        public ActionResult Error() {
            throw new Exception();
        }

        public class Heartbeat {
            public string Response { get; set; }
        }
    }
}