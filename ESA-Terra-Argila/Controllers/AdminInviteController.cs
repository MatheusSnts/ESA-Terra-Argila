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
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ESA_Terra_Argila.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminInviteController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<User> _userManager;

        public AdminInviteController(ApplicationDbContext context, UserManager<User> userManager, IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        [HttpPost("SendInvitation")]
        public async Task<IActionResult> SendInvitation([FromBody] InvitationRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest("O e-mail não pode estar vazio.");
            }


            var tokenBytes = RandomNumberGenerator.GetBytes(32);
            var rawToken = Convert.ToBase64String(tokenBytes);
            var encodedToken = WebEncoders.Base64UrlEncode(tokenBytes);

            var invitation = new Invitation
            {
                Email = request.Email,
                Token = encodedToken,
                ExpirationDate = DateTime.UtcNow.AddDays(7),
                Used = false
            };

            _context.Invitations.Add(invitation);
            await _context.SaveChangesAsync();

            var callbackUrl = $"{Request.Scheme}://{Request.Host}/api/AdminInvite/Register?token={encodedToken}&email={request.Email}";

            var subject = "Convite para Registo no Sistema";
            var message = $@"
                <p>Olá,</p>
                <p>Você foi convidado para se Registar no sistema.</p>
                <p>Clique no link abaixo para completar o seu Registo:</p>
                <p><a href='{callbackUrl}'>Completar Registo</a></p>
                <p>Este link expirará em 7 dias.</p>";

            await _emailSender.SendEmailAsync(request.Email, subject, message);

            return Ok("Convite enviado com sucesso.");
        }

        [HttpGet("Register")]
        public async Task<IActionResult> Register(string token, string email)
        {


            var invitation = await _context.Invitations
                .FirstOrDefaultAsync(i => i.Email == email && i.Token == token);


            if (invitation == null || invitation.Used || invitation.ExpirationDate < DateTime.UtcNow)
            {

                return BadRequest("Convite inválido ou expirado.");
            }
            invitation.Used = true;

            await _context.SaveChangesAsync();


            return Redirect($"/Identity/Account/Register?email={email}&token={token}");

        }

    }
}

public class InvitationRequest
{
    public string Email { get; set; }
}