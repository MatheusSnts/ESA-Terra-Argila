using ESA_Terra_Argila.Data;
using ESA_Terra_Argila.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ESA_Terra_Argila.Middleware
{

    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public LoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, ApplicationDbContext dbContext)
        {
           
                var log = new AccessLog
                {
                Timestamp = DateTime.UtcNow,
                IP = context.Connection.RemoteIpAddress?.ToString(),
                Path = context.Request.Path,
                Method = context.Request.Method,
                UserName = "zxzx"
                };
            if (log.Path.StartsWith("/Identity/Account/Login") || log.Path.StartsWith("/Identity/Account/Logout"))
            {
                dbContext.AccessLogs.Add(log);
                await dbContext.SaveChangesAsync();
            }
                await _next(context);
            
        }
    }}
