﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using UnitTestRealWorld.Web.Controllers;
using UnitTestRealWorld.Web.Models;
using Xunit;

namespace UnitTestRealWorld.Test
{
    public class ProductControllerWithInMemoryTest : ProductControllerTest
    {
        public ProductControllerWithInMemoryTest()
        {
            SetContextOptions(new DbContextOptionsBuilder<UnitTestDBContext>().UseInMemoryDatabase("UnitTestInMemoryDB").Options);
        }
        [Fact]
        public async Task Create_ValidModelState_ReturnRedirectToAction()
        {
            var newProduct = new Product { Name = "Oliver Twist", Price = 200, Stock = 2000, Color = "Red", };
            using (var context = new UnitTestDBContext(_contextOptions))
            {
                var category = context.Categories.First();

                newProduct.CategoryId = category.Id;

                var controller = new ProductsController(context);

                var result = await controller.Create(newProduct);

                var redirect = Assert.IsType<RedirectToActionResult>(result);
                Assert.Equal("Index", redirect.ActionName);
            }

            using (var context = new UnitTestDBContext(_contextOptions))
            {
                var product = context.Products.FirstOrDefault(x => x.Name == newProduct.Name);

                Assert.Equal(newProduct.Name, product.Name);
            }

        }

        [Theory]
        [InlineData(1)]
        public async Task DeleteCategory_ExistCategoryId_DeleteAllProducts(int categoryId)
        {
            using (var context = new UnitTestDBContext(_contextOptions))
            {
                var category = await context.Categories.FindAsync(categoryId);

                context.Categories.Remove(category);

                context.SaveChanges();
            }

            using (var context = new UnitTestDBContext(_contextOptions))
            {
                var products = await context.Products.Where(x => x.CategoryId == categoryId).ToListAsync();
                Assert.Empty(products);
            }
        }

    }
}