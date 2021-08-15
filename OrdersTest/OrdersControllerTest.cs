using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orders.Models;
using Orders.Persistance;
using Orders.Persistance.EF;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderTest
{
    [TestClass]
    public class OrdersControllerTest
    {
        [TestMethod]
        public async Task ReturnNullForMissingId()
        {
            System.IO.File.Delete("ReturnNullForMissingId.db");
            string ConnStr = "Data Source=ReturnNullForMissingId.db;";
            using var db = new DataContext(new DbContextOptionsBuilder<DataContext>().UseSqlite(ConnStr).Options);
            var productStorage = new ProductStorage(db);
            var orderStorage = new OrderStorage(db);
            var discountStorage = new ProductDiscountStorage(db);

            var controller = new Orders.Controllers.OrdersController(null, orderStorage, productStorage, discountStorage);
            var Rsp = await controller.GetOrder(System.Guid.NewGuid());
            Assert.IsNull(Rsp.Value);
        }

        [TestMethod]
        public async Task CanPutOrder()
        {
            System.IO.File.Delete("CanPutOrder.db");
            string ConnStr = "Data Source=CanPutOrder.db;";
            using var db = new DataContext(new DbContextOptionsBuilder<DataContext>().UseSqlite(ConnStr).Options);
            var productStorage = new ProductStorage(db);
            var orderStorage = new OrderStorage(db);
            var discountStorage = new ProductDiscountStorage(db);

            var controller = new Orders.Controllers.OrdersController(null, orderStorage, productStorage, discountStorage);
            var data = GetTestData().ToList();
            foreach(var i in data)
            {
                await productStorage.Insert(i.Key);
                await discountStorage.Insert(i.Value);
            }

            Orders.Models.External.ExternalOrderReq Req = new();
            Req.ProductOrders.Add(data[0].Key.Id, 1);
            Req.ProductOrders.Add(data[1].Key.Id, 5);
            Req.ProductOrders.Add(data[2].Key.Id, 3);
            var response = await controller.PutOrder(Req);
            Guid id = response.Value;
            var order = (await controller.GetOrder(id)).Value;

            var subtotal = 2706m;
            /*1 @ 15 = 15
              5 @ 34 with 0.1 discount = 153
              3 @ 940 with 0.1 discount = 2538
              15 + 153 + 2538 = 2706
            */

            Assert.AreEqual(3, order.ProductOrders.Count);
            Assert.AreEqual(subtotal, order.Subtotal);
            Assert.AreEqual(subtotal * (1 + Orders.Data.Constants.Tax), order.Total);
        }

        IEnumerable<KeyValuePair<Product, ProductDiscount>> GetTestData()
        {
            foreach (Product i in GetTestProducts())
            {
                decimal discount = 0.1m;
                yield return new(i, new() { Id = Guid.NewGuid(), Discount = discount, ProductId = i.Id, Threshold = 2 });
            }
        }

        IEnumerable<Product> GetTestProducts()
        {
            yield return new() { Id = Guid.NewGuid(), Title = "Inception", Description = "A super amazing movie.", Price = 15 };
            yield return new() { Id = Guid.NewGuid(), Title = "Frodo Baggins", Description = "A hyper realistic statue of Frodo Baggins.", Price = 34 };
            yield return new() { Id = Guid.NewGuid(), Title = "SQL Server", Description = "A database server.", Price = 940 };
            yield return new() { Id = Guid.NewGuid(), Title = "Johnnie Walker Gold Label Reserve", Description = "A refined and refreshing whisky.", Price = 80 };
        }
    }
}
