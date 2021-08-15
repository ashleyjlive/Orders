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
    public class ProductDiscountTest
    {
        [TestMethod]
        public async Task ReturnNullForMissingId()
        {
            System.IO.File.Delete("ReturnNullForMissingId.db");
            string ConnStr = "Data Source=ReturnNullForMissingId.db;";
            using var db = new DataContext(new DbContextOptionsBuilder<DataContext>().UseSqlite(ConnStr).Options);
            var discountsStorage = new ProductDiscountStorage(db);

            Assert.IsNull(await discountsStorage.Get(Guid.NewGuid()));
        }

        [TestMethod]
        public async Task CanInsertDiscount()
        {
            System.IO.File.Delete("CanInsertDiscount.db");
            string ConnStr = "Data Source=CanInsertDiscount.db;";
            using var db = new DataContext(new DbContextOptionsBuilder<DataContext>().UseSqlite(ConnStr).Options);
            var discountsStorage = new ProductDiscountStorage(db);

            KeyValuePair<Product, ProductDiscount> data = GetTestData().First();

            await discountsStorage.Insert(data.Value);
        }

        [TestMethod]
        public async Task CannotInsertTwice()
        {
            System.IO.File.Delete("CannotInsertTwice.db");
            string ConnStr = "Data Source=CannotInsertTwice.db;";
            using var db = new DataContext(new DbContextOptionsBuilder<DataContext>().UseSqlite(ConnStr).Options);
            var discountsStorage = new ProductDiscountStorage(db);

            KeyValuePair<Product, ProductDiscount> data = GetTestData().First();

            await discountsStorage.Insert(data.Value);
            await Assert.ThrowsExceptionAsync<ArgumentException>(async() => { await discountsStorage.Insert(data.Value); });
        }

        [TestMethod]
        public async Task CanInsertAndRetrieve()
        {
            System.IO.File.Delete("CanInsertAndRetrieve.db");
            string ConnStr = "Data Source=CanInsertAndRetrieve.db;";
            using var db = new DataContext(new DbContextOptionsBuilder<DataContext>().UseSqlite(ConnStr).Options);
            var discountsStorage = new ProductDiscountStorage(db);

            KeyValuePair<Product, ProductDiscount> data = GetTestData().First();

            await discountsStorage.Insert(data.Value);
            Assert.AreEqual(data.Value, await discountsStorage.Get(data.Value.ProductId));
        }

        [TestMethod]
        public async Task CanDelete()
        {
            System.IO.File.Delete("CanDelete.db");
            string ConnStr = "Data Source=CanDelete.db;";
            using var db = new DataContext(new DbContextOptionsBuilder<DataContext>().UseSqlite(ConnStr).Options);
            var discountsStorage = new ProductDiscountStorage(db);
            KeyValuePair<Product, ProductDiscount> data = GetTestData().First();
            await discountsStorage.Insert(data.Value);
            Assert.AreEqual(data.Value, await discountsStorage.Get(data.Value.ProductId));

            bool Success = await discountsStorage.Delete(data.Value.ProductId);
            Assert.IsTrue(Success);
            Assert.IsNull(await discountsStorage.Get(data.Value.ProductId));
        }

        [TestMethod]
        public async Task CanUpsert()
        {
            System.IO.File.Delete("CanUpsert.db");
            string ConnStr = "Data Source=CanUpsert.db;";
            using var db = new DataContext(new DbContextOptionsBuilder<DataContext>().UseSqlite(ConnStr).Options);
            var discountsStorage = new ProductDiscountStorage(db);
            KeyValuePair<Product, ProductDiscount> data = GetTestData().First();
            await discountsStorage.Insert(data.Value);
            Assert.AreEqual(data.Value, await discountsStorage.Get(data.Value.ProductId));

            data.Value.Discount = 0.1m;
            await discountsStorage.Upsert(data.Value);
            ProductDiscount discount = await discountsStorage.Get(data.Value.ProductId);
            Assert.AreEqual(discount, data.Value);
        }

        IEnumerable<KeyValuePair<Product, ProductDiscount>> GetTestData()
        {
            Random rand = new();
            foreach (Product i in GetTestProducts())
            {
                decimal discount = rand.Next(0, 99) / 100;
                yield return new(i, new() { Id = Guid.NewGuid(), Discount = discount, ProductId = i.Id });
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
