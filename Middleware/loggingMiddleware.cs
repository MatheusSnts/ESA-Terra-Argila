using ESA_Terra_Argila.Data;
using ESA_Terra_Argila.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace ESA_Terra_Argila.Middleware
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ApplicationDbContext _dbContext;

        public LoggingMiddleware(RequestDelegate next, ApplicationDbContext dbContext)
        {
            _next = next;
            _dbContext = dbContext;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Captura informações da requisição
                var ipAddress = context.Connection.RemoteIpAddress?.ToString();
                var path = context.Request.Path;
                var method = context.Request.Method;
                var timestamp = DateTime.UtcNow;

                // Verifica se a rota é crítica (ex.: login/logout)
                if (path.StartsWithSegments("/login") || path.StartsWithSegments("/logout"))
                {
                    var log = new AccessLog
                    {
                        IP = ipAddress,
                        Path = path,
                        Method = method,
                        Timestamp = timestamp,
                        
                    };

                    // Salvar no banco de dados
                    _dbContext.AccessLogs.Add(log);
                    await _dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                // Log de erros no middleware (se necessário)
                Console.WriteLine($"Erro no middleware de logging: {ex.Message}");
            }

            // Continua o pipeline
            await _next(context);
        }
    }
}
