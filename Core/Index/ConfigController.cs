using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Pulse.Api.Controllers.V2 {
    [Route("[controller]")]
    [ApiController]
    public class ResourceController : ControllerBase {
        [HttpGet]
        [Route("")]
        public ActionResult<object> Get() {
            var options = new JsonSerializerOptions {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            var jsonString = System.IO.File.ReadAllText("webpage-config.json");
            var jsonModel = JsonSerializer.Deserialize<object>(jsonString, options);

            return jsonModel;
        }
    }
}