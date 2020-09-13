using System;
using System.Net;

namespace Pulse.Core.AppErrors {
  public class AppException : Exception {
    public readonly HttpStatusCode StatusCode;
    public AppException(HttpStatusCode statusCode) : base() {
      StatusCode = statusCode;
    }
    public AppException(HttpStatusCode statusCode, string message) : base(message) {
      StatusCode = statusCode;
    }
  }
}