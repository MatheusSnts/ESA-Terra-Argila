using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using ESA_Terra_Argila.Data;
using ESA_Terra_Argila.Models;
using ESA_Terra_Argila.Services;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace ESA_Terra_Argila.Controllers
{
    public class AdminInviteController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailSender _emailSender;
        private readonly UserManager<User> _userManager;

        public AdminInviteController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
           
            _userManager = userManager;
        }

        /// <summary>
        /// Envia um convite para um e-mail específico.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SendInvitation(string email)
        {
            // Gerar um token seguro
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            // Criar um novo convite no banco de dados
            var invitation = new Invitation
            {
                Email = email,
                Token = encodedToken,
                ExpirationDate = DateTime.UtcNow.AddDays(7),
                Used = false
            };

            _context.Invitations.Add(invitation);
            await _context.SaveChangesAsync();

            // Criar o link de convite
            var callbackUrl = Url.Action("Register", "AdminInvite", new { token = encodedToken, email }, protocol: Request.Scheme);

            // Enviar o convite por e-mail
            await _emailSender.SendInvitationEmailAsync(email, callbackUrl);

            return Ok("Convite enviado com sucesso.");
        }

        /// <summary>
        /// Verifica se o convite é válido e redireciona para o formulário de registro.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Register(string token, string email)
        {
            var invitation = await _context.Invitations
                .FirstOrDefaultAsync(i => i.Email == email && i.Token == token);

            if (invitation == null || invitation.Used || invitation.ExpirationDate < DateTime.UtcNow)
            {
                return BadRequest("Convite inválido ou expirado.");
            }

            // Redirecionar para a página de cadastro
            return RedirectToPage("/Account/Register", new { email, token });
        }

        /// <summary>
        /// Completa o registro de um usuário convidado.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CompleteRegistration(string email, string token, string password,string fullName)
        {
            var invitation = await _context.Invitations
                .FirstOrDefaultAsync(i => i.Email == email && i.Token == token);

            if (invitation == null || invitation.Used || invitation.ExpirationDate < DateTime.UtcNow)
            {
                return BadRequest("Convite inválido ou expirado.");
            }

            // Criar o usuário no Identity
            var user = new User { UserName = email, Email = email, EmailConfirmed = true, FullName = fullName };
            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                // Marcar o convite como usado
                invitation.Used = true;
                await _context.SaveChangesAsync();

                return Ok("Cadastro concluído com sucesso.");
            }

            return BadRequest("Erro ao criar conta.");
        }
    }
}
