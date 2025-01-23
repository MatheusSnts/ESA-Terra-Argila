using ESA_Terra_Argila.Data;
using ESA_Terra_Argila.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ESA_Terra_Argila.Middleware
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public LoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IServiceProvider serviceProvider)
        {
            var ip = context.Connection.RemoteIpAddress?.ToString();
            var path = context.Request.Path;
            var method = context.Request.Method;
            var timestamp = DateTime.UtcNow;

            try
            {
                await _next(context);
            }
            finally
            {
                if (path.StartsWithSegments("/login") || path.StartsWithSegments("/logout"))
                {
                    using (var scope = serviceProvider.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                        var log = new AccessLog
                        {
                            IP = ip,
                            Path = path,
                            Method = method,
                            Timestamp = timestamp,

                        };

                        await dbContext.AccessLogs.AddAsync(log);
                        await dbContext.SaveChangesAsync();
                    }
                }
            }
        }
    }
}
