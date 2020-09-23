using System.Net;

namespace Pulse.Core.AppErrors {
  public class InternalException : AppException {
    public InternalException() : base(HttpStatusCode.InternalServerError) {}
    public InternalException(string message) : base(HttpStatusCode.InternalServerError, message) {}
  }
}