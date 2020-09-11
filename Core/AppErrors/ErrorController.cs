using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Pulse.Core.Authorization;
using Pulse.Core.Notifications;

namespace Pulse.Core.AppErrors {
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("[controller]")]
    [ApiController]
    public class ErrorController : ControllerBase {
        private readonly IWebHostEnvironment _env;
        private readonly AppErrorService _appErrorService;
        private readonly EmailService _emailService;

        public ErrorController(IWebHostEnvironment env, AppErrorService appErrorService, EmailService emailService) {
            _env = env;
            _appErrorService = appErrorService;
            _emailService = emailService;
        }

        [Route("")]
        public string Error() {
            var context = HttpContext.Features.Get<IExceptionHandlerFeature>();
            var ex = context?.Error;

            if (!_env.IsDevelopment()) {
                _emailService.SendException(ex);
            }

            _appErrorService.Add(ex);

            if (ex is AuthException) {
                Response.StatusCode = (int) HttpStatusCode.Unauthorized;
                return ex.Message;
            } else {
                Response.StatusCode = (int) HttpStatusCode.InternalServerError;
                return "";
            }
        }
    }
}