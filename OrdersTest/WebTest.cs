using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Orders.Models;
using Orders.Models.External;
using Orders.Persistance;
using Orders.Persistance.EF;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace OrderTest
{
    [TestClass]
    public class WebTest : WebApplicationFactory<Orders.Startup>
    {
        [TestMethod]
        public async Task ReturnNullForMissingOrder()
        {
            System.IO.File.Delete("web_data.db");
            using var client = CreateClient();

            var foo = await client.GetAsync("/orders/82E5B08D-C02B-40A6-B5AB-0E0A3C5F6C6A");
            Assert.IsFalse(foo.IsSuccessStatusCode);
        }

        [TestMethod]
        public async Task CanInsertProduct()
        {
            System.IO.File.Delete("web_data.db");
            using var client = CreateClient();

            ExternalProductReq req =
                new() { Description = "Description", Price = 40.0m, Title = "A product" };
            var content = new StringContent(JsonConvert.SerializeObject(req), System.Text.Encoding.UTF8, "application/json");
            var rsp = await client.PutAsync("/products", content);
            Assert.IsTrue(rsp.IsSuccessStatusCode);
        }

        [TestMethod]
        public async Task CanFetchProduct()
        {
            System.IO.File.Delete("web_data.db");
            using var client = CreateClient();

            ExternalProductReq req =
                new() { Description = "Description", Price = 40.0m, Title = "A product" };
            var content = new StringContent(JsonConvert.SerializeObject(req), System.Text.Encoding.UTF8, "application/json");
            var rsp = await client.PutAsync("/products", content);
            string Id = await rsp.Content.ReadAsStringAsync();
            Assert.IsTrue(rsp.IsSuccessStatusCode);

            var getRsp = await client.GetAsync("/products/" + Id.Trim('\"'));
            var responseContent = await getRsp.Content.ReadAsStringAsync();
            Product product = JsonConvert.DeserializeObject<Product>(responseContent);
            Assert.AreEqual("A product", product.Title);
            Assert.IsTrue(rsp.IsSuccessStatusCode);
        }

        [TestMethod]
        public async Task CanFormatOrderText()
        {
            System.IO.File.Delete("web_data.db");
            using var client = CreateClient();

            Guid ProductId = await PutProduct(client, "Product 1", "A description", 50.0m);
            Guid ProductId2 = await PutProduct(client, "Product 2", "Another description", 25.0m);

            await PutDiscount(client, ProductId, 0.1m, 2);
            await PutDiscount(client, ProductId2, 0.05m, 2);

            var productOrders = new Dictionary<Guid, uint>();
            productOrders.Add(ProductId, 4);
            productOrders.Add(ProductId2, 7);
            Guid orderId = await PutOrder(client, productOrders);

            Order order = await GetOrder(client, orderId);
            string html = await GetOrderHTML(client, orderId);
            string expectedHtml =
                "<html><body><h1>Order Reciept</h1><h3>Order Id: {0}</h3><h3>Order Time: {1}</h3><h3>Products Ordered</h3>" +
                "<table style=\"width:30%\"><tr><th>Name</th><th>Product Price</th><th>Quantity</th><th>Discount</th><th>Sum</th></tr>" +
                "<tr><td>Product 1</td><td>$50.00</td><td>4</td><td>10.00%</td><td>$180.00</td></tr><tr><td>Product 2</td><td>$25.00</td><td>7</td><td>5.00%</td><td>$166.25</td>" +
                "</tr></table><h3>Total savings: $28.75</h3><h3>Tax: $34.63</h3><h3>Subtotal: $346.25</h3><h3>Total: $380.88</h3></body></html>";
            Assert.AreEqual(string.Format(expectedHtml, order.Id, order.Time.ToLocalTime().ToString()), html);

            string plainText = await GetOrderText(client, orderId);
            string expectedText = 
                "Order Id: {0}\r\nTime: {1}\r\n\r\nProducts: \r\n" +
                "• Product 1 (x4) - $50.00 each at 10.00% off\r\n" +
                "• Product 2 (x7) - $25.00 each at 5.00% off\r\n\r\n" + 
                "Total savings: $28.75\r\nTax: $34.63\r\nSubtotal: $346.25\r\nTotal: $380.88";
            Assert.AreEqual(string.Format(expectedText, order.Id, order.Time.ToLocalTime().ToString()), plainText);
        }

        async Task<Guid> PutProduct(HttpClient client, string Title, string Description, decimal Price)
        {
            ExternalProductReq req =
                new() { Description = Description, Title = Title, Price = Price };
            var content = new StringContent(JsonConvert.SerializeObject(req), System.Text.Encoding.UTF8, "application/json");
            var rsp = await client.PutAsync("/products", content);
            string Id = await rsp.Content.ReadAsStringAsync();
            Assert.IsTrue(rsp.IsSuccessStatusCode);
            return Guid.Parse(Id.Trim('\"'));
        }

        async Task PutDiscount(HttpClient client, Guid ProductId, decimal Discount, uint Threshold)
        {
            ExternalDiscountReq req =
                new() {  ProductId = ProductId, Discount = Discount, Threshold = Threshold};
            var content = new StringContent(JsonConvert.SerializeObject(req), System.Text.Encoding.UTF8, "application/json");
            var rsp = await client.PutAsync("/discounts", content);
            Assert.IsTrue(rsp.IsSuccessStatusCode);
        }

        async Task<Guid> PutOrder(HttpClient client, Dictionary<Guid, uint> ProductOrders)
        {
            ExternalOrderReq req =
                new() { ProductOrders = ProductOrders };
            var content = new StringContent(JsonConvert.SerializeObject(req), System.Text.Encoding.UTF8, "application/json");
            var rsp = await client.PutAsync("/orders", content);
            string Id = await rsp.Content.ReadAsStringAsync();
            Assert.IsTrue(rsp.IsSuccessStatusCode);
            return Guid.Parse(Id.Trim('\"'));
        }

        async Task<Order> GetOrder(HttpClient client, Guid OrderId)
        {
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var rsp = await client.GetAsync("/orders/" + OrderId.ToString());
            Assert.IsTrue(rsp.IsSuccessStatusCode);
            string output = await rsp.Content.ReadAsStringAsync();
            Order order = JsonConvert.DeserializeObject<Order>(output);
            return order;
        }

        async Task<string> GetOrderHTML(HttpClient client, Guid OrderId)
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
            var rsp = await client.GetAsync("/orders/" + OrderId.ToString());
            Assert.IsTrue(rsp.IsSuccessStatusCode);
            string output = await rsp.Content.ReadAsStringAsync();
            return output;
        }

        async Task<string> GetOrderText(HttpClient client, Guid OrderId)
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
            var rsp = await client.GetAsync("/orders/" + OrderId.ToString());
            Assert.IsTrue(rsp.IsSuccessStatusCode);
            string output = await rsp.Content.ReadAsStringAsync();
            return output;
        }
    }
}
