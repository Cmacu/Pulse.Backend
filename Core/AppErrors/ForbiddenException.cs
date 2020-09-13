using System.Net;

namespace Pulse.Core.AppErrors {
  public class ForbiddenException : AppException {
    public ForbiddenException() : base(HttpStatusCode.Forbidden) {}
    public ForbiddenException(string message) : base(HttpStatusCode.NotFound, message) {}
  }
}