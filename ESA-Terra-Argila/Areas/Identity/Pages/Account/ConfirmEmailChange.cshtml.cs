// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using ESA_Terra_Argila.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace ESA_Terra_Argila.Areas.Identity.Pages.Account
{
    public class ConfirmEmailChangeModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IEmailSender _emailSender; // Adicionado para envio de email

        public ConfirmEmailChangeModel(UserManager<User> userManager, SignInManager<User> signInManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender; // Inicializando o serviço de e-mail
        }

        [TempData]
        public string StatusMessage { get; set; }




        public async Task<IActionResult> OnGetAsync(string userId, string email, string code)
        {
            if (userId == null || email == null || code == null)
            {
                return RedirectToPage("/Index");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{userId}'.");
            }
            System.Diagnostics.Debug.WriteLine("Antes de ChangeEmailAsync");
            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ChangeEmailAsync(user, email, code);
            if (!result.Succeeded)
            {
                StatusMessage = "Error changing email.";
                return Page();
            }
            System.Diagnostics.Debug.WriteLine("Ant de ChangeEmailAsync");
            var setUserNameResult = await _userManager.SetUserNameAsync(user, email);
            if (!setUserNameResult.Succeeded)
            {
                StatusMessage = "Error changing user name.";
                return Page();
            }

            await _signInManager.RefreshSignInAsync(user);



            // **Novo trecho para enviar um e-mail de confirmação**
            var confirmationMessage = $"Your email has been successfully changed to {email}.";
            await _emailSender.SendEmailAsync(
                email,
                "Email Changed Successfully",
                confirmationMessage
            );

            StatusMessage = "Thank you for confirming your email change. A confirmation email has been sent.";
            Console.WriteLine($"Email enviado para {email}.");
            return Page();
        }
    }
}