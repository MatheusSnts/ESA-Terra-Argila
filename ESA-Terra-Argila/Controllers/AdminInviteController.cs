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
    /// <summary>
    /// Controller responsável pelo gerenciamento de convites administrativos.
    /// Permite que administradores enviem convites para novos usuários se registrarem no sistema.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AdminInviteController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<User> _userManager;

        /// <summary>
        /// Inicializa uma nova instância do controller de convites administrativos.
        /// </summary>
        /// <param name="context">Contexto do banco de dados da aplicação.</param>
        /// <param name="userManager">Gerenciador de usuários do Identity.</param>
        /// <param name="emailSender">Serviço para envio de emails.</param>
        public AdminInviteController(ApplicationDbContext context, UserManager<User> userManager, IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        /// <summary>
        /// Envia um convite por email para um novo usuário se registrar no sistema.
        /// </summary>
        /// <param name="request">Requisição contendo o email do usuário a ser convidado.</param>
        /// <returns>Ok se o convite for enviado com sucesso, ou BadRequest se houver erro.</returns>
       
        [HttpPost("SendInvitation")]
        public async Task<IActionResult> SendInvitation([FromBody] InvitationRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Role))
            {
                return BadRequest("Email e role são obrigatórios.");
            }

            var userExists = await _userManager.FindByEmailAsync(request.Email);
            if (userExists != null)
            {
                return BadRequest("Já existe um utilizador com este email.");
            }

            
            var password = GenerateSecurePassword();
            var user = new User
            {
                Email = request.Email,
                UserName = request.Email,
                FullName = "Novo Utilizador",
                EmailConfirmed = true,
                AcceptedByAdmin = true
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            // Atribuir role
            if (!await _userManager.IsInRoleAsync(user, request.Role))
            {
                await _userManager.AddToRoleAsync(user, request.Role);
            }

            // Gerar token
            var tokenBytes = RandomNumberGenerator.GetBytes(32);
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

            var callbackUrl = $"{Request.Scheme}://{Request.Host}/Identity/Account/Manage?email={request.Email}&token={encodedToken}";

            var subject = "Convite para Registo no Sistema";
            var message = $@"
        <p>Olá,</p>
        <p>Você foi convidado a se registar no sistema Terra&Argila.</p>
        <p>Sua senha temporária é: <strong>{password}</strong></p>
        <p>Por favor clique no link abaixo para completar e editar o seu perfil:</p>
        <p><a href='{callbackUrl}'/a></p>
        <p>Este link expirará em 7 dias.</p>";

            await _emailSender.SendEmailAsync(request.Email, subject, message);

            return Ok("Convite enviado com sucesso.");
        }


        /// <summary>
        /// Processa o registro de um novo usuário através de um convite válido.
        /// </summary>
        /// <param name="token">Token de validação do convite.</param>
        /// <param name="email">Email do usuário convidado.</param>
        /// <returns>Redirecionamento para a página de registro ou BadRequest se o convite for inválido.</returns>
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

        /// <summary>
        /// renderiza a view para enviar o convite a um fornecedor
        /// </summary>
        [HttpGet("InviteSupplier")]
        public IActionResult InviteSupplier()
        {
            return View("~/Views/Admin/InviteSupplier.cshtml");
        }

        /// <summary>
        /// renderiza a view para enviar o convite a um vendedor
        /// </summary>
        [HttpGet("InviteVendor")]
        public IActionResult InviteVendor()
        {
            return View("~/Views/Admin/InviteVendor.cshtml");
        }

        /// <summary>
        /// Gera uma palavra-passe segura conforme os requisitos do Identity.
        /// </summary>
        private string GenerateSecurePassword()
        {
            const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lower = "abcdefghijklmnopqrstuvwxyz";
            const string digits = "0123456789";
            const string special = "!@#$%^&*_-+=";

            var random = new Random();
            var password = new StringBuilder();

            // Garante pelo menos um de cada tipo
            password.Append(upper[random.Next(upper.Length)]);
            password.Append(lower[random.Next(lower.Length)]);
            password.Append(digits[random.Next(digits.Length)]);
            password.Append(special[random.Next(special.Length)]);

            // Preenche com mais caracteres
            var allChars = upper + lower + digits + special;
            for (int i = 0; i < 8; i++)
            {
                password.Append(allChars[random.Next(allChars.Length)]);
            }

            return password.ToString();
        }
    }

    /// <summary>
    /// Modelo para a requisição de envio de convite administrativo.
    /// </summary>
    public class InvitationRequest
    {
        /// <summary>
        /// Email do usuário a ser convidado.
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// Role da conta, fornecedor ou empresa
        /// </summary>
        public string Role { get; set; } // "Supplier" ou "Vendor"
    }
}