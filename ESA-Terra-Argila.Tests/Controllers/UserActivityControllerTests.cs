using Xunit;
using Moq;
using ESA_Terra_Argila.Controllers;
using ESA_Terra_Argila.Services;
using ESA_Terra_Argila.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;

namespace ESA_Terra_Argila.Tests.Controllers
{
    public class UserActivityControllerTests
    {
        private readonly Mock<IUserActivityService> _mockService;
        private readonly UserActivityController _controller;

        public UserActivityControllerTests()
        {
            // Cria o mock do serviço
            _mockService = new Mock<IUserActivityService>();

            // Instancia o controller, injetando o mock
            _controller = new UserActivityController(_mockService.Object);
        }

        [Fact]
        public async Task Index_SemUserId_DeveRetornarChallenge()
        {
            // Arrange
            var emptyIdentity = new ClaimsIdentity(); // nenhum claim
            var emptyPrincipal = new ClaimsPrincipal(emptyIdentity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = emptyPrincipal }
            };

            // Act
            var result = await _controller.Index();

            // Assert
            Assert.IsType<ChallengeResult>(result);
        }

        [Fact]
        public async Task Index_ComUserId_DeveRetornarView()
        {
            // Arrange
            var userIdentity = new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.NameIdentifier, "usuario123") },
                "TestAuthType"
            );
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(userIdentity) }
            };

            var fakeActivities = new List<UserActivity>
            {
                new UserActivity { Id = 1, UserId = "usuario123", ActivityType = "Login" },
                new UserActivity { Id = 2, UserId = "usuario123", ActivityType = "Logout" }
            };

            _mockService
                .Setup(s => s.GetUserActivitiesAsync(
                    It.Is<string>(u => u == "usuario123"),
                    It.IsAny<string>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<DateTime?>()))
                .ReturnsAsync(fakeActivities);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<UserActivity>>(viewResult.Model);
            Assert.Equal(fakeActivities.Count, model.Count());
            _mockService.Verify(
                s => s.GetUserActivitiesAsync(
                    It.Is<string>(u => u == "usuario123"),
                    It.IsAny<string>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<DateTime?>()),
                Times.Once
            );
        }

        [Fact]
        public async Task UserActivities_SemUserId_DeveRetornarBadRequest()
        {
            // Arrange
            string? userId = null;

            // Act
            var result = await _controller.UserActivities(userId!);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("ID do usuário não fornecido", badRequest.Value);
        }

        [Fact]
        public async Task UserActivities_NaoAutorizado_DeveRetornarForbid()
        {
            // Arrange
            string userId = "usuario123";
            _mockService
                .Setup(s => s.IsAuthorizedToViewActivitiesAsync(
                    It.IsAny<ClaimsPrincipal>(),
                    userId))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.UserActivities(userId);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task UserActivities_Autorizado_DeveRetornarViewIndex()
        {
            // Arrange
            string userId = "usuario123";
            _mockService
                .Setup(s => s.IsAuthorizedToViewActivitiesAsync(
                    It.IsAny<ClaimsPrincipal>(),
                    userId))
                .ReturnsAsync(true);

            var fakeActivities = new List<UserActivity>
            {
                new UserActivity { Id = 1, UserId = userId, ActivityType = "Login" }
            };
            _mockService
                .Setup(s => s.GetUserActivitiesAsync(
                    It.Is<string>(u => u == userId),
                    It.IsAny<string>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<DateTime?>()))
                .ReturnsAsync(fakeActivities);

            // Act
            var result = await _controller.UserActivities(userId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Index", viewResult.ViewName);

            var model = Assert.IsAssignableFrom<IEnumerable<UserActivity>>(viewResult.Model);
            Assert.Single(model);
            _mockService.Verify(
                s => s.GetUserActivitiesAsync(
                    It.Is<string>(u => u == userId),
                    It.IsAny<string>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<DateTime?>()),
                Times.Once
            );
        }
    }
}
