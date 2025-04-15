using ESA_Terra_Argila.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace ESA_Terra_Argila.Middlewares
{
    /// <summary>
    /// Middleware que verifica se o usuário está bloqueado em cada requisição autenticada.
    /// Se o usuário estiver bloqueado, ele é desconectado e redirecionado para a página de bloqueio.
    /// </summary>
    public class LockoutCheckMiddleware
    {
        private readonly RequestDelegate _next;

        public LockoutCheckMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, UserManager<User> userManager, SignInManager<User> signInManager)
        {
            // Verifique se o usuário está autenticado
            if (context.User.Identity?.IsAuthenticated == true)
            {
                // Obtenha o usuário
                var user = await userManager.GetUserAsync(context.User);
                
                if (user != null)
                {
                    // Verifique se o usuário está bloqueado (LockoutEnd é no futuro)
                    if (user.LockoutEnabled && user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow)
                    {
                        // Usuário está bloqueado, faça logout e redirecione para a página de bloqueio
                        await signInManager.SignOutAsync();
                        
                        // Redireciona para a página de bloqueio
                        context.Response.Redirect("/Identity/Account/Lockout");
                        return;
                    }
                }
            }

            // Continue com a próxima etapa do pipeline
            await _next(context);
        }
    }

    // Classe de extensão para facilitar o registro do middleware
    public static class LockoutCheckMiddlewareExtensions
    {
        public static IApplicationBuilder UseLockoutCheck(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<LockoutCheckMiddleware>();
        }
    }
} 