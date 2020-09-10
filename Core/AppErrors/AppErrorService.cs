using System;
using Pulse.Backend;

namespace Pulse.Core.AppErrors {
    public class AppErrorService {
        private readonly DataContext _context;

        public AppErrorService(DataContext context) {
            _context = context;
        }

        public int Add(Exception ex) {
            var error = new AppError() {
                Timestamp = DateTime.UtcNow,
                Message = ex.Message,
                Details = ex.ToString()
            };

            _context.AppErrors.Add(error);
            _context.SaveChanges();

            return error.Id;
        }
    }
}