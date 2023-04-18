using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.SessionState;
using WingtipToys.Logic;
using Moq;
using WingtipToys.Interfaces;
using System.Linq;
using WingtipToys.Models;
using System.Data.Entity;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace WingtipToys.Tests
{
    [TestClass]
    public class ShoppingCartActionsTest
    {

        [TestInitialize]
        public void SetUp()
        {
            HttpContext.Current = FakeHttpContext();
        }

        [TestMethod]
        public void ShoppingCartTest_NewItemAddedtoDB()
        {
            //SetUp
            var CartId = "Test123";
            var mockDb = GenerateMockDatabase();
            var ProductId = 1;

            HttpContext.Current.Session["CartId"] = CartId;

            //Action
            var shoppingCartAction = new ShoppingCartActions(mockDb);
            shoppingCartAction.AddToCart(ProductId);

            //Assert
            var Result = mockDb.ShoppingCartItems.Where(c => c.CartId == CartId).ToList()[0];

            Assert.AreEqual(CartId, Result.CartId);
            Assert.AreEqual(ProductId, Result.ProductId);
            Assert.IsInstanceOfType(Result.ItemId, typeof(string));
            Assert.AreEqual(1, Result.Quantity);
        }

        [TestMethod]
        public void ShoppingCartTest_ExistingItemCountUpdated()
        {
            //SetUp
            var CartId = "Test123";
            var ProductId = 1;

            var mockDb = GenerateMockDatabase(new List<CartItem>
            {
            //ProductId is not the right product causing a new item to be added rather than quantity to be increased.
            new CartItem{ItemId = "1", CartId = CartId, ProductId = 2, Quantity = 3, DateCreated = DateTime.Now }
            });

            HttpContext.Current.Session["CartId"] = CartId;
            var shoppingCartAction = new ShoppingCartActions(mockDb);

            //Action
            shoppingCartAction.AddToCart(ProductId);

            //Assert
            Assert.AreEqual(4, mockDb.ShoppingCartItems.Where(
                c => c.CartId == CartId).ToList()[0].Quantity);
        }


        [TestMethod]
        public void ShoppingCartTest_TotalDoesNotGetsDiscountAppliedOnOrderUnder20()
        {
            //SetUp
            var CartId = "Test123";
            var UnitPrice = 15;

            var mockDb = GenerateMockDatabase(new List<CartItem>
            {
                new CartItem{ItemId = "1", CartId = CartId, ProductId = 2, Quantity = 1, DateCreated = DateTime.Now, Product = CreateProduct(UnitPrice)}
            });

            HttpContext.Current.Session["CartId"] = CartId;
            var shoppingCartAction = new ShoppingCartActions(mockDb);

            //Action
            var Result = shoppingCartAction.GetTotal();

            //Assert
            Assert.AreEqual(UnitPrice, Result * .85m);
        }

        //Helpers
        private static HttpContext FakeHttpContext()
        {
            var httpRequest = new HttpRequest("", "http://example.com/", "");
            var stringWriter = new StringWriter();
            var httpResponse = new HttpResponse(stringWriter);
            var httpContext = new HttpContext(httpRequest, httpResponse);

            var sessionContainer = new HttpSessionStateContainer("id", new SessionStateItemCollection(),
                                                    new HttpStaticObjectsCollection(), 10, true,
                                                    HttpCookieMode.AutoDetect,
                                                    SessionStateMode.InProc, false);

            httpContext.Items["AspSession"] = typeof(HttpSessionState).GetConstructor(
                                        BindingFlags.NonPublic | BindingFlags.Instance,
                                        null, CallingConventions.Standard,
                                        new[] { typeof(HttpSessionStateContainer) },
                                        null)
                                .Invoke(new object[] { sessionContainer });

            return httpContext;
        }

        private static IProductContext GenerateMockDatabase(List<CartItem> sourceList = null)
        {
            ProductContext context = new ProductContext();
            context.ShoppingCartItems = new Mock<DbSet<CartItem>>().Object;
            sourceList = sourceList ?? new List<CartItem>();
            context.ShoppingCartItems = GetQueryableMockDbSet<CartItem>(sourceList);

            return context;
        }


        private static DbSet<T> GetQueryableMockDbSet<T>(List<T> sourceList) where T : class
        {
            var queryable = sourceList.AsQueryable();

            var dbSet = new Mock<DbSet<T>>();
            dbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            dbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            dbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            dbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());
            dbSet.Setup(d => d.Add(It.IsAny<T>())).Callback<T>((s) => sourceList.Add(s));

            return dbSet.Object;
        }

        private static Product CreateProduct(double price)
        {
            return new Product
            {
                CategoryID = 1,
                Description = "There's nothing old about this toy car, except it's looks. Compatible with other old toy cars.",
                ImagePath = "carearly.png",
                ProductID = 1,
                ProductName = "Old-time Car",
                UnitPrice = price
            };
        }

    }
}
