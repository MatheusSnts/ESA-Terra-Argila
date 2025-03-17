// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using ESA_Terra_Argila.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using ESA_Terra_Argila.Data;

namespace ESA_Terra_Argila.Areas.Identity.Pages.Account
{
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<LogoutModel> _logger;

        public LogoutModel(SignInManager<User> signInManager, ILogger<LogoutModel> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }

        public async Task<IActionResult> OnPost(string returnUrl = null)
        {
            var userEmail = User.Identity.IsAuthenticated ? User.Identity.Name : "Unknown User";

            using (var scope = HttpContext.RequestServices.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.LogEntries.Add(new LogEntry
                {
                    UserEmail = userEmail,
                    Action = "Logout",
                    Timestamp = DateTime.UtcNow,
                    Ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown"
                });
                await dbContext.SaveChangesAsync();
            }

            await _signInManager.SignOutAsync();
            _logger.LogInformation($"User {userEmail} logged out at {DateTime.UtcNow}.");

            if (returnUrl != null)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                return RedirectToPage();
            }
        }


    }
}
