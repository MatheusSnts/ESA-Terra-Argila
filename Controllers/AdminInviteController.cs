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
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;

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

           
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            
            var invitation = new Invitation
            {
                Email = request.Email,
                Token = encodedToken,
                ExpirationDate = DateTime.UtcNow.AddDays(7),
                Used = false
            };

            _context.Invitations.Add(invitation);
            await _context.SaveChangesAsync();

      
            var callbackUrl = Url.Action("Register", "AdminInvite", new { token = encodedToken, email = request.Email }, protocol: Request.Scheme);


            var subject = "Convite para Cadastro no Sistema";
            var message = $@"
        <p>Olá,</p>
        <p>Você foi convidado para se cadastrar no sistema.</p>
        <p>Clique no link abaixo para completar seu cadastro:</p>
        <p><a href='{callbackUrl}'>Completar Cadastro</a></p>
        <p>Este link expirará em 7 dias.</p>";

            await _emailSender.SendEmailAsync(request.Email, subject, message);
            
            return Ok("Convite enviado com sucesso.");
        }


        [HttpGet]
        public async Task<IActionResult> Register(string token, string email)
        {
            var invitation = await _context.Invitations
                .FirstOrDefaultAsync(i => i.Email == email && i.Token == token);

            if (invitation == null || invitation.Used || invitation.ExpirationDate < DateTime.UtcNow)
            {
                return BadRequest("Convite inválido ou expirado.");
            }

      
            return RedirectToPage("/Account/Register", new { email, token });
        }

        [HttpPost]
        public async Task<IActionResult> CompleteRegistration(string email, string token, string password,string fullName)
        {
            var invitation = await _context.Invitations
                .FirstOrDefaultAsync(i => i.Email == email && i.Token == token);

            if (invitation == null || invitation.Used || invitation.ExpirationDate < DateTime.UtcNow)
            {
                return BadRequest("Convite inválido ou expirado.");
            }

        
            var user = new User { UserName = email, Email = email, EmailConfirmed = true, FullName = fullName };
            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
          
                invitation.Used = true;
                await _context.SaveChangesAsync();

                return Ok("Cadastro concluído com sucesso.");
            }

            return BadRequest("Erro ao criar conta.");
        }
    }
}
public class InvitationRequest
{
    public string Email { get; set; }
}
