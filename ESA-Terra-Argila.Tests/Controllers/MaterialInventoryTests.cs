using Xunit;
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
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.Extensions.Logging;
using ESA_Terra_Argila.Areas.Identity.Pages.Account.Manage;
using Castle.Core.Smtp;
using ESA_Terra_Argila.Areas.Identity.Pages.Account;

namespace ESA_Terra_Argila.Tests.Controllers
{
    public class MaterialInventoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly MaterialsController _controller;
        private readonly List<Material> _materials;
        private readonly Category _category;
        private readonly string _userId = "test-user-id";
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<EmailModel> _mockEmailModel;

        public MaterialInventoryTests()
        {
            // Configurar o banco de dados em memória com um nome único para cada teste
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("TestMaterialInventoryDb_" + Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);

            // Configurar o UserManager Mock
            var userStore = new Mock<IUserStore<User>>();
            _mockUserManager = new Mock<UserManager<User>>(
                userStore.Object, null, null, null, null, null, null, null, null);
            
            // Configurar o comportamento do UserManager
            _mockUserManager.Setup(x => x.GetUserIdAsync(It.IsAny<User>()))
                .ReturnsAsync(_userId);
            _mockUserManager.Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns(_userId);
            _mockUserManager.Setup(x => x.FindByIdAsync(_userId))
                .ReturnsAsync(new User { Id = _userId, FullName = "Test User" });
            _mockUserManager.Setup(x => x.GetRolesAsync(It.IsAny<User>()))
                .ReturnsAsync(new List<string> { "Supplier" });

            // Criar e adicionar uma categoria para os materiais
            _category = new Category
            {
                Id = 1,
                Name = "Categoria Teste",
                UserId = _userId,
                Reference = "CAT001"
            };
            _context.Categories.Add(_category);
            _context.SaveChanges();

            // Dados de exemplo para materiais
            _materials = new List<Material>
            {
                new Material
                {
                    Id = 1,
                    Name = "Material 1",
                    Reference = "M001",
                    Description = "Descrição do Material 1",
                    Price = 50.0f,
                    Unit = "kg",
                    CreatedAt = DateTime.Now.AddDays(-10),
                    UserId = _userId,
                    CategoryId = _category.Id,
                    Category = _category
                },
                new Material
                {
                    Id = 2,
                    Name = "Material 2",
                    Reference = "M002",
                    Description = "Descrição do Material 2",
                    Price = 75.0f,
                    Unit = "l",
                    CreatedAt = DateTime.Now.AddDays(-5),
                    UserId = _userId,
                    CategoryId = _category.Id,
                    Category = _category
                }
            };

            // Adicionar materiais ao contexto
            _context.Materials.AddRange(_materials);
            _context.SaveChanges();

            // Configurar o usuario para o controller
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, _userId),
            }, "mock"));

            _mockEmailModel = new Mock<EmailModel>(_mockUserManager.Object, Mock.Of<SignInManager<User>>(), Mock.Of<IEmailSender>(), Mock.Of<ILogger<ExternalLoginModel>>());

            // Configurar o controller com o context real e o usuario
            _controller = new MaterialsController(_context, _mockUserManager.Object, _mockEmailModel.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = user }
                },
                TempData = new TempDataDictionary(
                    new DefaultHttpContext(), 
                    Mock.Of<ITempDataProvider>())
            };
            
            // Acionar explicitamente o OnActionExecuting para definir o userId
            _controller.OnActionExecuting(new ActionExecutingContext(
                new ActionContext(
                    _controller.ControllerContext.HttpContext,
                    new RouteData(),
                    new ActionDescriptor()),
                new List<IFilterMetadata>(),
                new Dictionary<string, object>(),
                null));
        }

        public void Dispose()
        {
            // Limpar o banco de dados após cada teste
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task Index_DeveListarTodosMateriaisDoUsuario()
        {
            // Garantir que o usuário e as relações estão definidos corretamente
            foreach (var material in _materials)
            {
                material.UserId = _userId;
            }
            await _context.SaveChangesAsync();

            // Verificar se os materiais estão realmente no banco com o UserId correto
            var materiaisNoBanco = await _context.Materials
                .Where(m => m.UserId == _userId)
                .ToListAsync();
            Assert.Equal(2, materiaisNoBanco.Count);

            // Configurar um novo controlador para o teste de Index
            // Isso garante que o userId e claims estão configurados corretamente
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, _userId),
            }, "mock"));

            var controller = new MaterialsController(_context, _mockUserManager.Object, _mockEmailModel.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = user }
                },
                TempData = new TempDataDictionary(
                    new DefaultHttpContext(),
                    Mock.Of<ITempDataProvider>())
            };
            
            // Chamar explicitamente o OnActionExecuting para definir o userId
            controller.OnActionExecuting(new ActionExecutingContext(
                new ActionContext(
                    controller.ControllerContext.HttpContext,
                    new RouteData(),
                    new ActionDescriptor()),
                new List<IFilterMetadata>(),
                new Dictionary<string, object>(),
                null));
            
            // Configurar o mock do UserManager para retornar um usuário específico
            _mockUserManager
                .Setup(x => x.FindByIdAsync(_userId))
                .ReturnsAsync(new User { Id = _userId, FullName = "Test User" });
            
            // Configurar o mock do UserManager para retornar roles
            _mockUserManager
                .Setup(x => x.GetRolesAsync(It.IsAny<User>()))
                .ReturnsAsync(new List<string> { "Supplier" }); // Para garantir que não é role "Vendor"

            // Act
            var result = await controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Material>>(viewResult.Model);
            Assert.Equal(2, model.Count());
        }

        [Fact]
        public async Task Create_DeveAdicionarNovoMaterial()
        {
            // Arrange
            var novoMaterial = new Material
            {
                Name = "Novo Material",
                Reference = "NM001",
                Description = "Descrição do Novo Material",
                Price = 120.0f,
                Unit = "kg",
                CategoryId = _category.Id
            };

            // Act
            var result = await _controller.Create(novoMaterial, new List<IFormFile>(), new List<int>());

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            
            var materialCriado = await _context.Materials.FirstOrDefaultAsync(m => 
                m.Name == novoMaterial.Name && m.Reference == novoMaterial.Reference);
            Assert.NotNull(materialCriado);
            Assert.Equal(_userId, materialCriado.UserId);
        }

        [Fact]
        public async Task Edit_DeveAtualizarMaterialExistente()
        {
            // Arrange
            var materialId = 1;
            
            // Preparar o ambiente para o teste Edit
            // Primeiro, precisamos obter o material do EditGet
            var getResult = await _controller.Edit(materialId);
            var viewResult = Assert.IsType<ViewResult>(getResult);
            var materialOriginal = Assert.IsType<Material>(viewResult.Model);
            Assert.NotNull(materialOriginal);
            
            var materialAtualizado = new Material
            {
                Id = materialId,
                CategoryId = _category.Id,
                Name = "Material Atualizado",
                Reference = "M001-UPDATED",
                Description = "Descrição atualizada",
                Price = 60.0f,
                Unit = "kg"
            };

            // Act
            var result = await _controller.Edit(materialId, materialAtualizado, new List<IFormFile>(), new List<int>());

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            
            var materialEditado = await _context.Materials.FindAsync(materialId);
            Assert.NotNull(materialEditado);
            Assert.Equal("Material Atualizado", materialEditado.Name);
            Assert.Equal("M001-UPDATED", materialEditado.Reference);
        }

        [Fact]
        public async Task Delete_DeveRemoverMaterial()
        {
            // Arrange
            int materialId = 1;
            
            // Verificar se o material existe antes de tentar excluir
            var materialExistente = await _context.Materials
                .Include(m => m.Category)
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.Id == materialId);
            Assert.NotNull(materialExistente);

            // Primeiro, obtemos a view de confirmação de exclusão
            var getResult = await _controller.Delete(materialId);
            Assert.NotNull(getResult);
            
            // Depois, confirmamos a exclusão
            var confirmResult = await _controller.DeleteConfirmed(materialId);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(confirmResult);
            Assert.Equal("Index", redirectResult.ActionName);
            
            var materialRemovido = await _context.Materials.FindAsync(materialId);
            Assert.Null(materialRemovido);
        }

        [Fact]
        public async Task Details_DeveExibirDetalhesDeMaterial()
        {
            // Arrange
            int materialId = 1;

            // Act
            var result = await _controller.Details(materialId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Material>(viewResult.Model);
            Assert.Equal(materialId, model.Id);
            Assert.Equal("Material 1", model.Name);
        }
    }
} 