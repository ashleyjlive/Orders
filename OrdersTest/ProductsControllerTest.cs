using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orders.Models;
using Orders.Models.External;
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
    public class ProductsControllerTest
    {
        [TestMethod]
        public async Task ReturnNullForMissingId()
        {
            System.IO.File.Delete("ReturnNullForMissingId.db");
            string ConnStr = "Data Source=ReturnNullForMissingId.db;";
            using var db = new DataContext(new DbContextOptionsBuilder<DataContext>().UseSqlite(ConnStr).Options);
            var productStorage = new ProductStorage(db);

            var controller = new Orders.Controllers.ProductsController(null, productStorage);
            var Rsp = await controller.GetProduct(System.Guid.NewGuid());
            Assert.IsNull(Rsp.Value);
        }

        [TestMethod]
        public async Task CanPopulateProducts()
        {
            System.IO.File.Delete("CanPopulateProducts.db");
            string ConnStr = "Data Source=CanPopulateProducts.db;";
            using var db = new DataContext(new DbContextOptionsBuilder<DataContext>().UseSqlite(ConnStr).Options);
            var productStorage = new ProductStorage(db);

            var controller = new Orders.Controllers.ProductsController(null, productStorage);
            foreach(ExternalProductReq prod in GetTestProducts())
            {
                _ = await controller.PutProduct(prod);
            }
        }

        [TestMethod]
        public async Task CanPopulateAndReadProducts()
        {
            System.IO.File.Delete("CanPopulateAndReadProducts.db");
            string ConnStr = "Data Source=CanPopulateAndReadProducts.db;";
            using var db = new DataContext(new DbContextOptionsBuilder<DataContext>().UseSqlite(ConnStr).Options);
            var productStorage = new ProductStorage(db);

            var controller = new Orders.Controllers.ProductsController(null, productStorage);

            Dictionary<Guid, ExternalProductReq> ProdIds = new();
            
            foreach (ExternalProductReq prod in GetTestProducts())
            {
                Guid id = (await controller.PutProduct(prod)).Value;
                ProdIds.Add(id, prod);
            }

            foreach (KeyValuePair<Guid, ExternalProductReq> KV in ProdIds)
            {
                Product product = (await controller.GetProduct(KV.Key)).Value;
                Assert.AreEqual(KV.Value.Title, product.Title);
            }
        }

        [TestMethod]
        public async Task CanUpdate()
        {
            System.IO.File.Delete("CanUpdate.db");
            string ConnStr = "Data Source=CanUpdate.db;";
            using var db = new DataContext(new DbContextOptionsBuilder<DataContext>().UseSqlite(ConnStr).Options);
            var productStorage = new ProductStorage(db);

            var controller = new Orders.Controllers.ProductsController(null, productStorage);

            List<ExternalProductReq> products = new(GetTestProducts());
            List<Guid> productIds = new();
            foreach (ExternalProductReq prod in products)
            {
                var id = (await controller.PutProduct(prod)).Value;
                productIds.Add(id);
            }

            var Prod0 = products.First();
            var Prod1 = (await controller.GetProduct(productIds.First())).Value;
            Assert.AreEqual(Prod0.Title, Prod1.Title);
            Prod1.Title = "Baz";
            await controller.PostProduct(Prod1);
            var Prod2 = (await controller.GetProduct(productIds.First())).Value;
            Assert.AreEqual("Baz", Prod2.Title);
        }

        IEnumerable<ExternalProductReq> GetTestProducts()
        {
            yield return new() { Title = "Inception", Description = "A super amazing movie.", Price = 15 };
            yield return new() { Title = "Frodo Baggins", Description = "A hyper realistic statue of Frodo Baggins.", Price = 34 };
            yield return new() { Title = "SQL Server", Description = "A database server.", Price = 940 };
            yield return new() { Title = "Johnnie Walker Gold Label Reserve", Description = "A refined and refreshing whisky.", Price = 80};
        }
    }
}
