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
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.Extensions.Logging;

namespace ESA_Terra_Argila.Tests.Controllers
{
    public class ProductInventoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly ProductsController _controller;
        private readonly List<Product> _products;
        private readonly Category _category;
        private readonly string _userId = "test-user-id";
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly ILogger<ProductsController> _logger;

        public ProductInventoryTests()
        {
            // Configurar o banco de dados em memória com um nome único para cada teste
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("TestProductInventoryDb_" + Guid.NewGuid().ToString())
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

            // Criar e adicionar uma categoria para os produtos
            _category = new Category
            {
                Id = 1,
                Name = "Categoria Teste",
                UserId = _userId,
                Reference = "CAT001"
            };
            _context.Categories.Add(_category);
            _context.SaveChanges();

            // Dados de exemplo para produtos
            _products = new List<Product>
            {
                new Product
                {
                    Id = 1,
                    Name = "Produto 1",
                    Reference = "P001",
                    Description = "Descrição do Produto 1",
                    Price = 100.0f,
                    Unit = "un",
                    CreatedAt = DateTime.Now.AddDays(-10),
                    UserId = _userId,
                    CategoryId = _category.Id,
                    Category = _category
                },
                new Product
                {
                    Id = 2,
                    Name = "Produto 2",
                    Reference = "P002",
                    Description = "Descrição do Produto 2",
                    Price = 150.0f,
                    Unit = "kg",
                    CreatedAt = DateTime.Now.AddDays(-5),
                    UserId = _userId,
                    CategoryId = _category.Id,
                    Category = _category
                }
            };

            // Adicionar produtos ao contexto
            _context.Items.AddRange(_products);
            _context.SaveChanges();

            // Configurar o usuario para o controller
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, _userId),
            }, "mock"));

            // Configurar o controller com o context real e o usuario
            _controller = new ProductsController(_context, _mockUserManager.Object, _logger)
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
        public async Task Index_DeveListarTodosProdutosDoUsuario()
        {
            // Garantir que o usuário e as relações estão definidos corretamente
            foreach (var product in _products)
            {
                product.UserId = _userId;
            }
            await _context.SaveChangesAsync();
            
            // Verificar se os produtos estão realmente no banco com o UserId correto
            var produtosNoBanco = await _context.Items
                .OfType<Product>()
                .Where(p => p.UserId == _userId)
                .ToListAsync();
            Assert.Equal(2, produtosNoBanco.Count);

            // Configurar um novo controlador para o teste de Index
            // Isso garante que o userId e claims estão configurados corretamente
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, _userId),
            }, "mock"));

            var controller = new ProductsController(_context, _mockUserManager.Object,_logger)
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
                .Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns(_userId);

            // Act
            var result = await controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Product>>(viewResult.Model);
            Assert.Equal(2, model.Count());
        }

        [Fact]
        public async Task Create_DeveAdicionarNovoProduto()
        {
            // Arrange
            var novoProduto = new Product
            {
                Name = "Novo Produto",
                Reference = "NP001",
                Description = "Descrição do Novo Produto",
                Price = 200.0f,
                Unit = "un",
                CategoryId = _category.Id
            };

            // Act
            var result = await _controller.Create(novoProduto, new List<IFormFile>(), new List<int>(), new List<int>(), new List<float?>());

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            
            var produtoCriado = await _context.Items.OfType<Product>().FirstOrDefaultAsync(p => 
                p.Name == novoProduto.Name && p.Reference == novoProduto.Reference);
            Assert.NotNull(produtoCriado);
            Assert.Equal(_userId, produtoCriado.UserId);
        }

        [Fact]
        public async Task Edit_DeveAtualizarProdutoExistente()
        {
            // Arrange
            var produtoId = 1;
            
            // Preparar o ambiente para o teste Edit
            // Primeiro, precisamos obter o produto do EditGet
            var getResult = await _controller.Edit(produtoId);
            var viewResult = Assert.IsType<ViewResult>(getResult);
            var produtoOriginal = Assert.IsType<Product>(viewResult.Model);
            Assert.NotNull(produtoOriginal);
            
            var produtoAtualizado = new Product
            {
                Id = produtoId,
                CategoryId = _category.Id,
                Name = "Produto Atualizado",
                Reference = "P001-UPDATED",
                Description = "Descrição atualizada",
                Price = 120.0f,
                Unit = "un"
            };

            // Act
            var result = await _controller.Edit(produtoId, produtoAtualizado, new List<IFormFile>(), new List<int>(), new List<int>(), new List<float?>());

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            
            var produtoEditado = await _context.Items.FindAsync(produtoId);
            Assert.NotNull(produtoEditado);
            Assert.Equal("Produto Atualizado", produtoEditado.Name);
            Assert.Equal("P001-UPDATED", produtoEditado.Reference);
        }

        [Fact]
        public async Task Delete_DeveRemoverProduto()
        {
            // Arrange
            int produtoId = 1;
            
            // Verificar se o produto existe antes de tentar excluir
            var produtoExistente = await _context.Items
                .OfType<Product>()
                .Include(p => p.Category)
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == produtoId);
            Assert.NotNull(produtoExistente);

            // Primeiro, obtemos a view de confirmação de exclusão
            var getResult = await _controller.Delete(produtoId);
            Assert.NotNull(getResult);
            
            // Depois, confirmamos a exclusão
            var confirmResult = await _controller.DeleteConfirmed(produtoId);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(confirmResult);
            Assert.Equal("Index", redirectResult.ActionName);
            
            var produtoRemovido = await _context.Items.FindAsync(produtoId);
            Assert.Null(produtoRemovido);
        }

        [Fact]
        public async Task Details_DeveExibirDetalhesDoProduto()
        {
            // Arrange
            int produtoId = 1;

            // Act
            var result = await _controller.Details(produtoId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Product>(viewResult.Model);
            Assert.Equal(produtoId, model.Id);
            Assert.Equal("Produto 1", model.Name);
        }
    }
} 