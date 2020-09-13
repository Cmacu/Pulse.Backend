using System.Net;

namespace Pulse.Core.AppErrors {
  public class NotFoundException : AppException {
    public NotFoundException() : base(HttpStatusCode.NotFound) {}
    public NotFoundException(string message) : base(HttpStatusCode.NotFound, message) {}
  }
}