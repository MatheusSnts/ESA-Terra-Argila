using Xunit;
using Moq;
using ESA_Terra_Argila.Models;
using ESA_Terra_Argila.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ESA_Terra_Argila.Controllers;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;

namespace ESA_Terra_Argila.Tests.Controllers
{
    public class AdminInviteControllerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly AdminInviteController _controller;
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<IEmailSender> _mockEmailSender;
        private readonly string _adminUserId = "admin-user-id";

        public AdminInviteControllerTests()
        {
            // Configurar o banco de dados em memória
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("TestAdminInviteDb_" + Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);

            // Configurar o UserManager Mock
            var userStore = new Mock<IUserStore<User>>();
            _mockUserManager = new Mock<UserManager<User>>(
                userStore.Object, null, null, null, null, null, null, null, null);

            // Configurar o comportamento do UserManager
            _mockUserManager.Setup(x => x.GetUserIdAsync(It.IsAny<User>()))
                .ReturnsAsync(_adminUserId);
            _mockUserManager.Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns(_adminUserId);
            _mockUserManager.Setup(x => x.FindByIdAsync(_adminUserId))
                .ReturnsAsync(new User { Id = _adminUserId, FullName = "Admin User" });

            // Configurar o EmailSender Mock
            _mockEmailSender = new Mock<IEmailSender>();
            _mockEmailSender.Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Configurar o controller
            var httpContext = new DefaultHttpContext();
            var urlHelper = new Mock<IUrlHelper>();
            urlHelper.Setup(x => x.Action(It.IsAny<UrlActionContext>()))
                .Returns("https://test.com/invite");

            _controller = new AdminInviteController(_context, _mockUserManager.Object, _mockEmailSender.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = httpContext
                },
                Url = urlHelper.Object,
                TempData = new TempDataDictionary(
                    httpContext,
                    Mock.Of<ITempDataProvider>())
            };

            // Configurar Request.Scheme e Request.Host
            httpContext.Request.Scheme = "https";
            httpContext.Request.Host = new HostString("test.com");
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task SendInvitation_EmailValido_DeveCriarConviteEEnviarEmail()
        {
            // Arrange
            var request = new InvitationRequest
            {
                Email = "test@example.com"
            };

            // Act
            var result = await _controller.SendInvitation(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Convite enviado com sucesso.", okResult.Value);

            // Verificar se o convite foi criado no banco de dados
            var convite = await _context.Invitations.FirstOrDefaultAsync(i => i.Email == request.Email);
            Assert.NotNull(convite);
            Assert.NotEmpty(convite.Token);
            Assert.False(convite.Used);
            Assert.True(convite.ExpirationDate > DateTime.UtcNow);

            // Verificar se o e-mail foi enviado
            _mockEmailSender.Verify(x => x.SendEmailAsync(
                It.Is<string>(e => e == request.Email),
                It.IsAny<string>(),
                It.IsAny<string>()
            ), Times.Once);
        }

        [Fact]
        public async Task SendInvitation_EmailInvalido_DeveRetornarBadRequest()
        {
            // Arrange
            var request = new InvitationRequest
            {
                Email = ""
            };

            // Act
            var result = await _controller.SendInvitation(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("O e-mail não pode estar vazio.", badRequestResult.Value);

            // Verificar que nenhum convite foi criado
            var conviteCount = await _context.Invitations.CountAsync();
            Assert.Equal(0, conviteCount);

            // Verificar que nenhum e-mail foi enviado
            _mockEmailSender.Verify(x => x.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()
            ), Times.Never);
        }

        [Fact]
        public async Task Register_ConviteValido_DeveRetornarRedirect()
        {
            // Arrange
            var email = "test@example.com";
            var token = "valid-token";
            
            var invitation = new Invitation
            {
                Email = email,
                Token = token,
                ExpirationDate = DateTime.UtcNow.AddDays(7),
                Used = false
            };
            
            _context.Invitations.Add(invitation);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.Register(token, email);

            // Assert
            var redirectResult = Assert.IsType<RedirectResult>(result);
            Assert.StartsWith("/Identity/Account/Register", redirectResult.Url);
            Assert.Contains($"email={email}", redirectResult.Url);
            Assert.Contains($"token={token}", redirectResult.Url);

            // Verificar que o convite foi marcado como usado
            var updatedInvitation = await _context.Invitations.FirstOrDefaultAsync(i => i.Email == email);
            Assert.NotNull(updatedInvitation);
            Assert.True(updatedInvitation.Used);
        }

        [Fact]
        public async Task Register_ConviteInexistente_DeveRetornarBadRequest()
        {
            // Act
            var result = await _controller.Register("invalid-token", "nonexistent@example.com");

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Convite inválido ou expirado.", badRequestResult.Value);
        }

        [Fact]
        public async Task Register_ConviteJaUsado_DeveRetornarBadRequest()
        {
            // Arrange
            var email = "test@example.com";
            var token = "used-token";
            
            var invitation = new Invitation
            {
                Email = email,
                Token = token,
                ExpirationDate = DateTime.UtcNow.AddDays(7),
                Used = true
            };
            
            _context.Invitations.Add(invitation);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.Register(token, email);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Convite inválido ou expirado.", badRequestResult.Value);
        }

        [Fact]
        public async Task Register_ConviteExpirado_DeveRetornarBadRequest()
        {
            // Arrange
            var email = "test@example.com";
            var token = "expired-token";
            
            var invitation = new Invitation
            {
                Email = email,
                Token = token,
                ExpirationDate = DateTime.UtcNow.AddDays(-1), // Expirado
                Used = false
            };
            
            _context.Invitations.Add(invitation);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.Register(token, email);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Convite inválido ou expirado.", badRequestResult.Value);
        }
    }
} 