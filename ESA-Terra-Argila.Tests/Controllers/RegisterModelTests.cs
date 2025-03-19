using Xunit;
using Moq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using ESA_Terra_Argila.Models;
using ESA_Terra_Argila.Areas.Identity.Pages.Account;
using System.Threading;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;

namespace ESA_Terra_Argila.Tests.Controllers
{
    public class TestUrlHelper : IUrlHelper
    {
        public ActionContext ActionContext { get; set; } = default!;

        public string Action(UrlActionContext actionContext)
        {
            return "/action-url";
        }

        public string? Content(string? contentPath)
        {
            return contentPath;
        }

        public bool IsLocalUrl(string? url)
        {
            return true;
        }

        public string? Link(string? routeName, object? values)
        {
            return "/link-url";
        }

        public string RouteUrl(UrlRouteContext routeContext)
        {
            return "/route-url";
        }
    }

    public class RegisterModelTests
    {
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<IUserStore<User>> _userStoreMock;
        private readonly Mock<SignInManager<User>> _signInManagerMock;
        private readonly Mock<ILogger<RegisterModel>> _loggerMock;
        private readonly Mock<IEmailSender> _emailSenderMock;
        private readonly RegisterModel _pageModel;

        public RegisterModelTests()
        {
            // Cria o mock do IUserStore para o UserManager
            _userStoreMock = new Mock<IUserStore<User>>();
            var emailStore = _userStoreMock.As<IUserEmailStore<User>>();
            emailStore.Setup(x => x.GetEmailAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((User user, CancellationToken token) => user.Email);
            emailStore.Setup(x => x.SetEmailAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            emailStore.Setup(x => x.GetUserIdAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((User user, CancellationToken token) => user.Id);
            emailStore.Setup(x => x.GetUserNameAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((User user, CancellationToken token) => user.UserName);
            emailStore.Setup(x => x.SetUserNameAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            emailStore.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(IdentityResult.Success);

            var identityOptions = new IdentityOptions();
            var optionsAccessor = new Mock<IOptions<IdentityOptions>>();
            optionsAccessor.Setup(o => o.Value).Returns(identityOptions);

            // Configura o UserManager com todos os parâmetros necessários
            _userManagerMock = new Mock<UserManager<User>>(
                _userStoreMock.Object,
                optionsAccessor.Object,
                new Mock<IPasswordHasher<User>>().Object,
                new List<IUserValidator<User>>(),
                new List<IPasswordValidator<User>>(),
                new Mock<ILookupNormalizer>().Object,
                new Mock<IdentityErrorDescriber>().Object,
                new Mock<IServiceProvider>().Object,
                new Mock<ILogger<UserManager<User>>>().Object
            );

            // Configura o suporte a email no UserManager
            _userManagerMock.Setup(x => x.SupportsUserEmail)
                .Returns(true);

            // Configura outros métodos necessários do UserManager
            _userManagerMock.Setup(x => x.GetUserIdAsync(It.IsAny<User>()))
                .ReturnsAsync("testUserId");
            _userManagerMock.Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<User>()))
                .ReturnsAsync("testToken");

            // Cria o SignInManager com os parâmetros necessários
            var contextAccessor = new Mock<IHttpContextAccessor>();
            var claimsFactory = new Mock<IUserClaimsPrincipalFactory<User>>();
            var loggerSignIn = new Mock<ILogger<SignInManager<User>>>();
            var schemes = new Mock<IAuthenticationSchemeProvider>();
            var confirmation = new Mock<IUserConfirmation<User>>();
            _signInManagerMock = new Mock<SignInManager<User>>(
                _userManagerMock.Object,
                contextAccessor.Object,
                claimsFactory.Object,
                optionsAccessor.Object,
                loggerSignIn.Object,
                schemes.Object,
                confirmation.Object
            );

            _loggerMock = new Mock<ILogger<RegisterModel>>();
            _emailSenderMock = new Mock<IEmailSender>();

            // Configura o HttpContext
            var httpContext = new DefaultHttpContext();
            var routeData = new RouteData();
            var actionDescriptor = new PageActionDescriptor();

            // Configura o ActionContext
            var actionContext = new ActionContext(
                httpContext,
                routeData,
                actionDescriptor
            );

            // Instancia a PageModel RegisterModel e configura o PageContext
            _pageModel = new RegisterModel(
                _userManagerMock.Object,
                _userStoreMock.Object,
                _signInManagerMock.Object,
                _loggerMock.Object,
                _emailSenderMock.Object)
            {
                PageContext = new PageContext(actionContext),
                Url = new TestUrlHelper { ActionContext = actionContext }
            };

            // Configura o TempData necessário para o PageModel
            var tempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(
                httpContext,
                Mock.Of<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider>()
            );
            _pageModel.TempData = tempData;
        }

        [Fact]
        public async Task OnPostAsync_ValidModel_ShouldCreateUserAndSignIn()
        {
            // Arrange
            _pageModel.Input = new RegisterModel.InputModel
            {
                FullName = "Test User",
                Email = "test@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                Role = "Customer"
            };

            _userManagerMock
                .Setup(um => um.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            string returnUrl = "/returnUrl";

            // Act
            var result = await _pageModel.OnPostAsync(returnUrl);

            // Assert
            var redirect = Assert.IsType<LocalRedirectResult>(result);
            Assert.Equal(returnUrl, redirect.Url);

            // Verificação modificada para não usar argumentos opcionais
            _userManagerMock.Verify(um => um.CreateAsync(
                It.IsAny<User>(),
                It.IsAny<string>()),
                Times.Once);
            _userManagerMock.Verify(um => um.AddToRoleAsync(It.IsAny<User>(), "Customer"), Times.Once);

            // Usando uma expressão mais específica para SignInAsync
            Expression<Func<SignInManager<User>, Task>> signInExpression =
                sm => sm.SignInAsync(It.IsAny<User>(), false, null);
            _signInManagerMock.Verify(signInExpression, Times.Once);
        }

        [Fact]
        public async Task OnPostAsync_InvalidModel_ShouldRedisplayForm()
        {
            // Arrange
            _pageModel.Input = new RegisterModel.InputModel
            {
                FullName = "Abc",
                Email = "invalid-email",
                Password = "short",
                ConfirmPassword = "different", // senhas não conferem
                Role = "Customer"
            };

            // Força o ModelState a ficar inválido
            _pageModel.ModelState.AddModelError("Input.ConfirmPassword", "Passwords do not match");

            // Act
            var result = await _pageModel.OnPostAsync(returnUrl: "/");

            // Assert
            var pageResult = Assert.IsType<PageResult>(result);
            _userManagerMock.Verify(um => um.CreateAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task OnPostAsync_WhenUserCreationFails_ShouldShowErrors()
        {
            // Arrange
            _pageModel.Input = new RegisterModel.InputModel
            {
                FullName = "Test User",
                Email = "test@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                Role = "Customer"
            };

            _userManagerMock
                .Setup(um => um.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Fake error" }));

            string returnUrl = "/returnUrl";

            // Act
            var result = await _pageModel.OnPostAsync(returnUrl);

            // Assert
            var pageResult = Assert.IsType<PageResult>(result);
            Assert.False(_pageModel.ModelState.IsValid);
            // Verificação modificada para não usar argumentos opcionais
            _userManagerMock.Verify(um => um.CreateAsync(
                It.IsAny<User>(),
                It.IsAny<string>()),
                Times.Once);

            // Usando uma expressão mais específica para SignInAsync
            Expression<Func<SignInManager<User>, Task>> signInExpression2 =
                sm => sm.SignInAsync(It.IsAny<User>(), false, null);
            _signInManagerMock.Verify(signInExpression2, Times.Never);
        }
    }
}
