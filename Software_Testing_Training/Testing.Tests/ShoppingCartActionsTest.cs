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
            var mockDb = FakeProductContext();
            var ItemId = 1;

            HttpContext.Current.Session["CartId"] = CartId;
            var shoppingCartAction = new ShoppingCartActions(mockDb);

            //Action
            shoppingCartAction.AddToCart(ItemId);

            //Validation
            Assert.AreEqual(1, mockDb.ShoppingCartItems.Where(
                c => c.CartId == CartId).ToList().Count);
        }

        [TestMethod]
        public void ShoppingCartTest_ExistingItemCountUpdated()
        {
            //SetUp
            var CartId = "Test123";
            var ItemId = 1;

            var mockDb = FakeProductContext(new List<CartItem>
            {
                //ProductId is not the right product causing a new item to be added rather than quantity to be increased.

            new CartItem{ItemId = ItemId.ToString(), CartId = CartId, ProductId = 2, Quantity = 3, DateCreated = DateTime.Now }
            });

            HttpContext.Current.Session["CartId"] = CartId;
            var shoppingCartAction = new ShoppingCartActions(mockDb);

            //Action
            shoppingCartAction.AddToCart(ItemId);

            //Validation
            Assert.AreEqual(4, mockDb.ShoppingCartItems.Where(
                c => c.CartId == CartId).ToList()[0].Quantity);
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

        private static IProductContext FakeProductContext(List<CartItem> sourceList = null)
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
    }
}
