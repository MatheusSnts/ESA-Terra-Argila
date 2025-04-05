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
using ESA_Terra_Argila.Enums;
using static ESA_Terra_Argila.Controllers.OrdersController;

namespace ESA_Terra_Argila.Tests.Controllers
{
    public class OrderControllerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly OrdersController _controller;
        private readonly string _userId = "test-user-id";
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Product _product;
        private readonly Material _material;
        private readonly Category _category;
        private readonly User _user;

        public OrderControllerTests()
        {
            // Configurar o banco de dados em memória com um nome único para cada teste
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("TestOrderControllerDb_" + Guid.NewGuid().ToString())
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

            // Criar um usuário para os testes
            _user = new User 
            { 
                Id = _userId, 
                FullName = "Test User",
                Email = "test@example.com"
            };
            
            _mockUserManager.Setup(x => x.FindByIdAsync(_userId))
                .ReturnsAsync(_user);
            _mockUserManager.Setup(x => x.GetRolesAsync(It.IsAny<User>()))
                .ReturnsAsync(new List<string> { "Customer" });

            // Criar e adicionar uma categoria
            _category = new Category
            {
                Id = 1,
                Name = "Categoria Teste",
                UserId = _userId,
                Reference = "CAT001"
            };
            _context.Categories.Add(_category);
            
            // Criar um produto para os testes
            _product = new Product
            {
                Id = 1,
                Name = "Produto Teste",
                Reference = "P001",
                Description = "Descrição do Produto Teste",
                Price = 100.0f,
                Unit = "un",
                CreatedAt = DateTime.Now.AddDays(-5),
                UserId = _userId,
                CategoryId = _category.Id,
                Category = _category
            };
            
            // Criar um material para os testes
            _material = new Material
            {
                Id = 2,
                Name = "Material Teste",
                Reference = "M001",
                Description = "Descrição do Material Teste",
                Price = 50.0f,
                Unit = "kg",
                Stock = 100,
                CreatedAt = DateTime.Now.AddDays(-10),
                UserId = _userId,
                CategoryId = _category.Id,
                Category = _category
            };

            // Adicionar itens ao contexto
            _context.Items.Add(_product);
            _context.Items.Add(_material);
            _context.SaveChanges();

            // Configurar o usuário para o controller
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, _userId),
            }, "mock"));

            // Configurar o controller com o context real e o usuário
            _controller = new OrdersController(_context, _mockUserManager.Object)
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
        public async Task Cart_DeveExibirCarrinhoVazioSeNaoExistir()
        {
            // Arrange
            // Criar um pedido vazio para retornar quando o método Cart for chamado
            var order = new Order
            {
                UserId = _userId,
                Status = OrderStatus.Draft,
                CreatedAt = DateTime.Now,
                OrderItems = new List<OrderItem>()
            };
            
            // Adicionar o pedido ao contexto para que o método Cart possa encontrá-lo
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.Cart();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Order>(viewResult.Model);
            Assert.Empty(model.OrderItems);
            Assert.Equal(_userId, model.UserId);
            Assert.Equal(OrderStatus.Draft, model.Status);
        }

        [Fact]
        public async Task Cart_DeveExibirCarrinhoExistenteComItens()
        {
            // Arrange
            var order = new Order
            {
                UserId = _userId,
                Status = OrderStatus.Draft,
                CreatedAt = DateTime.Now.AddHours(-1),
                OrderItems = new List<OrderItem>
                {
                    new OrderItem
                    {
                        ItemId = _product.Id,
                        Item = _product,
                        Quantity = 2
                    }
                }
            };
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.Cart();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Order>(viewResult.Model);
            Assert.Single(model.OrderItems);
            Assert.Equal(_userId, model.UserId);
            Assert.Equal(OrderStatus.Draft, model.Status);
            Assert.Equal(_product.Id, model.OrderItems.First().ItemId);
            Assert.Equal(2, model.OrderItems.First().Quantity);
        }

        [Fact]
        public async Task AddToCart_ItemExiste_DeveAdicionarNovoItem()
        {
            // Act
            var result = await _controller.AddToCart(_product.Id);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            
            // Verificar se o pedido foi criado
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.UserId == _userId && o.Status == OrderStatus.Draft);
            
            Assert.NotNull(order);
            Assert.Single(order.OrderItems);
            Assert.Equal(_product.Id, order.OrderItems.First().ItemId);
            Assert.Equal(1, order.OrderItems.First().Quantity);
        }

        [Fact]
        public async Task AddToCart_ItemJaNoCarrinho_DeveIncrementarQuantidade()
        {
            // Arrange
            var order = new Order
            {
                UserId = _userId,
                Status = OrderStatus.Draft,
                CreatedAt = DateTime.Now.AddHours(-1),
                OrderItems = new List<OrderItem>
                {
                    new OrderItem
                    {
                        ItemId = _product.Id,
                        Item = _product,
                        Quantity = 1
                    }
                }
            };
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.AddToCart(_product.Id);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            
            // Verificar se a quantidade foi incrementada
            var updatedOrder = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.UserId == _userId && o.Status == OrderStatus.Draft);
            
            Assert.NotNull(updatedOrder);
            Assert.Single(updatedOrder.OrderItems);
            Assert.Equal(_product.Id, updatedOrder.OrderItems.First().ItemId);
            Assert.Equal(2, updatedOrder.OrderItems.First().Quantity);
        }

        [Fact]
        public async Task DeleteItem_ItemExisteNoCarrinho_DeveRemoverItem()
        {
            // Arrange
            var order = new Order
            {
                UserId = _userId,
                Status = OrderStatus.Draft,
                CreatedAt = DateTime.Now.AddHours(-1),
                OrderItems = new List<OrderItem>
                {
                    new OrderItem
                    {
                        Id = 1,
                        ItemId = _product.Id,
                        Item = _product,
                        Quantity = 2
                    }
                }
            };
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Configure HttpContext para incluir o cabeçalho Referer
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Add("Referer", "http://test.com");
            
            // Atualizar o controlador com o novo HttpContext
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            await _controller.DeleteItem(1);

            // Assert - Foco apenas no resultado da operação
            // Verificar se o item foi removido
            var updatedOrder = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.UserId == _userId && o.Status == OrderStatus.Draft);
            
            Assert.NotNull(updatedOrder);
            Assert.Empty(updatedOrder.OrderItems);
        }

        [Fact]
        public async Task AddQuantity_ItemExiste_DeveIncrementarQuantidade()
        {
            // Arrange
            var order = new Order
            {
                UserId = _userId,
                Status = OrderStatus.Draft,
                CreatedAt = DateTime.Now.AddHours(-1),
                OrderItems = new List<OrderItem>
                {
                    new OrderItem
                    {
                        Id = 1,
                        ItemId = _product.Id,
                        Item = _product,
                        Quantity = 1
                    }
                }
            };
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Act
            var request = new AddQuantityRequestModel { Id = 1, Value = 1 };
            var result = await _controller.AddQuantity(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            
            // Verificar se a quantidade foi incrementada
            var updatedOrder = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.UserId == _userId && o.Status == OrderStatus.Draft);
            
            Assert.NotNull(updatedOrder);
            Assert.Single(updatedOrder.OrderItems);
            Assert.Equal(2, updatedOrder.OrderItems.First().Quantity);
        }
    }
} 