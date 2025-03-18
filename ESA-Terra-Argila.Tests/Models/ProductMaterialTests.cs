using Xunit;
using ESA_Terra_Argila.Models;
using ESA_Terra_Argila.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ESA_Terra_Argila.Tests.Models
{
    public class ProductMaterialTests
    {
        [Fact]
        public void ProductMaterial_CriacaoComSucesso_DeveIniciarCorretamente()
        {
            // Arrange
            var productId = 1;
            var materialId = 10;

            // Act
            var productMaterial = new ProductMaterial
            {
                ProductId = productId,
                MaterialId = materialId
            };

            // Assert
            Assert.Equal(productId, productMaterial.ProductId);
            Assert.Equal(materialId, productMaterial.MaterialId);
        }

        [Fact]
        public void ProductMaterial_ComProdutoEMaterial_DeveManterReferenciaCorreta()
        {
            // Arrange
            var produto = new Product { Id = 1, Name = "Produto Teste" };
            var material = new Material { Id = 10, Name = "Material Teste" };

            // Act
            var productMaterial = new ProductMaterial
            {
                ProductId = produto.Id,
                MaterialId = material.Id,
                Product = produto,
                Material = material
            };

            // Assert
            Assert.Equal(produto, productMaterial.Product);
            Assert.Equal(material, productMaterial.Material);
            Assert.Equal(produto.Id, productMaterial.ProductId);
            Assert.Equal(material.Id, productMaterial.MaterialId);
        }
    }

    public class ProductMaterialRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly string _userId = "test-user-id";
        private readonly List<Product> _products;
        private readonly List<Material> _materials;
        private readonly List<ProductMaterial> _productMaterials;
        private readonly Category _category;

        public ProductMaterialRepositoryTests()
        {
            // Configurar o banco de dados em memória
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("TestProductMaterialDb_" + Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);

            // Criar e adicionar uma categoria
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
                new Product { 
                    Id = 1, 
                    Name = "Produto 1", 
                    UserId = _userId,
                    CategoryId = _category.Id,
                    Category = _category,
                    Reference = "P001",
                    Description = "Descrição do Produto 1",
                    Price = 100.0f,
                    Unit = "un",
                    CreatedAt = DateTime.Now.AddDays(-10)
                },
                new Product { 
                    Id = 2, 
                    Name = "Produto 2", 
                    UserId = _userId,
                    CategoryId = _category.Id,
                    Category = _category,
                    Reference = "P002",
                    Description = "Descrição do Produto 2",
                    Price = 200.0f,
                    Unit = "kg",
                    CreatedAt = DateTime.Now.AddDays(-5)
                }
            };

            // Dados de exemplo para materiais
            _materials = new List<Material>
            {
                new Material { 
                    Id = 1, 
                    Name = "Material 1", 
                    UserId = _userId,
                    CategoryId = _category.Id,
                    Category = _category,
                    Reference = "M001",
                    Description = "Descrição do Material 1",
                    Price = 50.0f,
                    Unit = "kg",
                    CreatedAt = DateTime.Now.AddDays(-10)
                },
                new Material { 
                    Id = 2, 
                    Name = "Material 2", 
                    UserId = _userId,
                    CategoryId = _category.Id,
                    Category = _category,
                    Reference = "M002",
                    Description = "Descrição do Material 2",
                    Price = 75.0f,
                    Unit = "l",
                    CreatedAt = DateTime.Now.AddDays(-5)
                },
                new Material { 
                    Id = 3, 
                    Name = "Material 3", 
                    UserId = _userId,
                    CategoryId = _category.Id,
                    Category = _category,
                    Reference = "M003",
                    Description = "Descrição do Material 3",
                    Price = 120.0f,
                    Unit = "g",
                    CreatedAt = DateTime.Now.AddDays(-2)
                }
            };

            // Adicionar produtos e materiais ao contexto
            _context.Products.AddRange(_products);
            _context.Materials.AddRange(_materials);
            _context.SaveChanges();

            // Dados de exemplo para a relação produto-material
            _productMaterials = new List<ProductMaterial>
            {
                new ProductMaterial {
                    Id = 1,
                    ProductId = 1,
                    MaterialId = 1,
                    Product = _products.First(p => p.Id == 1),
                    Material = _materials.First(m => m.Id == 1)
                },
                new ProductMaterial {
                    Id = 2,
                    ProductId = 1,
                    MaterialId = 2,
                    Product = _products.First(p => p.Id == 1),
                    Material = _materials.First(m => m.Id == 2)
                },
                new ProductMaterial {
                    Id = 3,
                    ProductId = 2,
                    MaterialId = 2,
                    Product = _products.First(p => p.Id == 2),
                    Material = _materials.First(m => m.Id == 2)
                }
            };

            // Adicionar relações ao contexto
            _context.ProductMaterials.AddRange(_productMaterials);
            _context.SaveChanges();
        }

        public void Dispose()
        {
            // Limpar o banco de dados após cada teste
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task ObterMateriaisDoProduto_DeveRetornarMateriaisCorretos()
        {
            // Arrange
            int produtoId = 1;

            // Act
            var materiais = await _context.ProductMaterials
                .Where(pm => pm.ProductId == produtoId)
                .Include(pm => pm.Material)
                .Select(pm => pm.Material)
                .ToListAsync();

            // Assert
            Assert.Equal(2, materiais.Count);
            Assert.Contains(materiais, m => m.Id == 1);
            Assert.Contains(materiais, m => m.Id == 2);
        }

        [Fact]
        public async Task ObterProdutosComMaterial_DeveRetornarProdutosCorretos()
        {
            // Arrange
            int materialId = 2;

            // Act
            var produtos = await _context.ProductMaterials
                .Where(pm => pm.MaterialId == materialId)
                .Include(pm => pm.Product)
                .Select(pm => pm.Product)
                .ToListAsync();

            // Assert
            Assert.Equal(2, produtos.Count);
            Assert.Contains(produtos, p => p.Id == 1);
            Assert.Contains(produtos, p => p.Id == 2);
        }

        [Fact]
        public async Task AdicionarMaterialAoProduto_DeveAdicionarRelacionamento()
        {
            // Arrange
            var novoProdutoMaterial = new ProductMaterial
            {
                Id = 4,
                ProductId = 2,
                MaterialId = 3,
                Product = _products.First(p => p.Id == 2),
                Material = _materials.First(m => m.Id == 3)
            };

            // Act
            _context.ProductMaterials.Add(novoProdutoMaterial);
            await _context.SaveChangesAsync();

            // Assert
            var produtoMaterial = await _context.ProductMaterials
                .FirstOrDefaultAsync(pm => pm.ProductId == 2 && pm.MaterialId == 3);
            Assert.NotNull(produtoMaterial);
        }

        [Fact]
        public async Task RemoverMaterialDoProduto_DeveRemoverRelacionamento()
        {
            // Arrange
            var productMaterialParaRemover = await _context.ProductMaterials
                .FirstOrDefaultAsync(pm => pm.ProductId == 1 && pm.MaterialId == 1);
        
            Assert.NotNull(productMaterialParaRemover); // Garantir que o item existe antes de remover

            // Act
            _context.ProductMaterials.Remove(productMaterialParaRemover);
            await _context.SaveChangesAsync();

            // Assert
            var produtoMaterial = await _context.ProductMaterials
                .FirstOrDefaultAsync(pm => pm.ProductId == 1 && pm.MaterialId == 1);
            Assert.Null(produtoMaterial);
        }
    }
} 