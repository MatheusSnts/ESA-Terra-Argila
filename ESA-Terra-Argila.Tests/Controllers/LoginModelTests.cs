using Xunit;
using Moq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication;
using ESA_Terra_Argila.Models;
using ESA_Terra_Argila.Areas.Identity.Pages.Account;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Threading.Tasks;

namespace ESA_Terra_Argila.Tests.Controllers
{
    public class LoginModelTests
    {
        private readonly Mock<SignInManager<User>> _signInManagerMock;
        private readonly Mock<ILogger<LoginModel>> _loggerMock;
        private readonly LoginModel _pageModel;
        private readonly Mock<UserManager<User>> _userManagerMock;

        public LoginModelTests()
        {
            // Configuração do UserManager (necessário para o SignInManager)
            var userStoreMock = new Mock<IUserStore<User>>();
            var optionsAccessorMock = new Mock<Microsoft.Extensions.Options.IOptions<IdentityOptions>>();
            optionsAccessorMock.Setup(o => o.Value).Returns(new IdentityOptions());
            var passwordHasherMock = new Mock<IPasswordHasher<User>>();
            var userValidatorMock = new[] { new Mock<IUserValidator<User>>().Object };
            var passwordValidatorMock = new[] { new Mock<IPasswordValidator<User>>().Object };
            var lookupNormalizerMock = new Mock<ILookupNormalizer>();
            var errorDescriberMock = new Mock<IdentityErrorDescriber>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerMock = new Mock<ILogger<UserManager<User>>>();

            _userManagerMock = new Mock<UserManager<User>>(
                userStoreMock.Object,
                optionsAccessorMock.Object,
                passwordHasherMock.Object,
                userValidatorMock,
                passwordValidatorMock,
                lookupNormalizerMock.Object,
                errorDescriberMock.Object,
                serviceProviderMock.Object,
                loggerMock.Object
            );

            // Configuração do SignInManager
            var contextAccessor = new Mock<IHttpContextAccessor>();
            var userPrincipalFactory = new Mock<IUserClaimsPrincipalFactory<User>>();
            var logger = new Mock<ILogger<SignInManager<User>>>();
            var optionsAccessorMock2 = new Mock<Microsoft.Extensions.Options.IOptions<IdentityOptions>>();
            optionsAccessorMock2.Setup(o => o.Value).Returns(new IdentityOptions());
            var authSchemeProviderMock = new Mock<Microsoft.AspNetCore.Authentication.IAuthenticationSchemeProvider>();
            var userConfirmationMock = new Mock<IUserConfirmation<User>>();

            _signInManagerMock = new Mock<SignInManager<User>>(
                _userManagerMock.Object,
                contextAccessor.Object,
                userPrincipalFactory.Object,
                optionsAccessorMock2.Object,
                logger.Object,
                authSchemeProviderMock.Object,
                userConfirmationMock.Object
            );

            _loggerMock = new Mock<ILogger<LoginModel>>();

            // Configuração do HttpContext e PageContext
            var httpContext = new DefaultHttpContext();
            var modelState = new Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary();
            var actionContext = new ActionContext(
                httpContext,
                new RouteData(),
                new PageActionDescriptor(),
                modelState
            );

            // Instancia o LoginModel
            _pageModel = new LoginModel(_signInManagerMock.Object, _loggerMock.Object)
            {
                PageContext = new PageContext(actionContext)
            };
        }

        [Fact]
        public async Task OnPostAsync_ValidCredentials_ShouldSignInAndRedirect()
        {
            // Arrange
            _pageModel.Input = new LoginModel.InputModel
            {
                Email = "test@example.com",
                Password = "Password123!",
                RememberMe = false
            };

            _signInManagerMock
                .Setup(x => x.PasswordSignInAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            string returnUrl = "/returnUrl";

            // Act
            var result = await _pageModel.OnPostAsync(returnUrl);

            // Assert
            var redirectResult = Assert.IsType<LocalRedirectResult>(result);
            Assert.Equal(returnUrl, redirectResult.Url);
            _signInManagerMock.Verify(
                x => x.PasswordSignInAsync(
                    "test@example.com",
                    "Password123!",
                    false,
                    true),
                Times.Once);
        }

        [Fact]
        public async Task OnPostAsync_InvalidCredentials_ShouldShowError()
        {
            // Arrange
            _pageModel.Input = new LoginModel.InputModel
            {
                Email = "test@example.com",
                Password = "WrongPassword",
                RememberMe = false
            };

            _signInManagerMock
                .Setup(x => x.PasswordSignInAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

            // Act
            var result = await _pageModel.OnPostAsync("/");

            // Assert
            var pageResult = Assert.IsType<PageResult>(result);
            Assert.False(_pageModel.ModelState.IsValid);
            Assert.Contains(_pageModel.ModelState.Keys, k => k == string.Empty);
            Assert.Contains(
                _pageModel.ModelState[string.Empty].Errors,
                e => e.ErrorMessage == "Tentativa de login inválida.");
        }

        [Fact]
        public async Task OnPostAsync_AccountLocked_ShouldRedirectToLockout()
        {
            // Arrange
            _pageModel.Input = new LoginModel.InputModel
            {
                Email = "test@example.com",
                Password = "Password123!",
                RememberMe = false
            };

            _signInManagerMock
                .Setup(x => x.PasswordSignInAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.LockedOut);

            // Act
            var result = await _pageModel.OnPostAsync("/");

            // Assert
            var redirectResult = Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("./Lockout", redirectResult.PageName);
        }

        [Fact]
        public async Task OnPostAsync_RequiresTwoFactor_ShouldRedirectTo2FA()
        {
            // Arrange
            _pageModel.Input = new LoginModel.InputModel
            {
                Email = "test@example.com",
                Password = "Password123!",
                RememberMe = false
            };

            _signInManagerMock
                .Setup(x => x.PasswordSignInAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.TwoFactorRequired);

            // Act
            var result = await _pageModel.OnPostAsync("/");

            // Assert
            var redirectResult = Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("./LoginWith2fa", redirectResult.PageName);
        }

        [Fact]
        public async Task OnPostAsync_InvalidModel_ShouldReturnPage()
        {
            // Arrange
            _pageModel.Input = new LoginModel.InputModel
            {
                Email = "invalid-email",
                Password = "",
                RememberMe = false
            };

            _pageModel.ModelState.AddModelError("Input.Email", "Email inválido");
            _pageModel.ModelState.AddModelError("Input.Password", "A senha é obrigatória");

            // Act
            var result = await _pageModel.OnPostAsync("/");

            // Assert
            var pageResult = Assert.IsType<PageResult>(result);
            Assert.False(_pageModel.ModelState.IsValid);
        }
    }
}