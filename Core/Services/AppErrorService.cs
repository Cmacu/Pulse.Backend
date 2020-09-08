using System;
using Pulse.Configuration;
using Pulse.Core.Entities;

namespace Pulse.Services.Core
{
    public interface IAppErrorService
    {
        int Add(Exception exception);
    }
    public class AppErrorService : IAppErrorService
    {
        private readonly DataContext _context;

        public AppErrorService(DataContext context)
        {
            _context = context;
        }

        public int Add(Exception ex)
        {
            var error = new AppError()
            {
                Timestamp = DateTime.UtcNow,
                Message = ex.Message,
                Details = ex.ToString()
            };

            _context.AppError.Add(error);
            _context.SaveChanges();

            return error.Id;
        }
    }
}