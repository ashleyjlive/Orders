using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Orders.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Orders.Formatters
{
    public class OrderFormatter : TextOutputFormatter
    {
        public OrderFormatter()
        {
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/html"));
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/plain"));

            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);
        }

        protected override bool CanWriteType(Type type)
        {
            return typeof(Order).IsAssignableFrom(type);
        }

        public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            var httpContext = context.HttpContext;
            var builder = new StringBuilder();

            switch (httpContext.Request.Headers["accept"])
            {
                case MediaTypeNames.Text.Html:
                    FormatHtmlOrder(builder, (Order)context.Object);
                    break;
                case MediaTypeNames.Text.Plain:
                    FormatTextOrder(builder, (Order)context.Object);
                    break;
                default:
                    throw new InvalidOperationException();
            }

            await httpContext.Response.Body.WriteAsync(selectedEncoding.GetBytes(builder.ToString()));
        }

        static void FormatTextOrder(StringBuilder Builder, Order Order)
        {
            Builder.Append("Order Id: ");
            Builder.AppendLine(Order.Id.ToString());
            Builder.Append("Time: ");
            Builder.AppendLine(Order.Time.ToLocalTime().ToString());
            Builder.AppendLine();
            Builder.AppendLine("Products: ");

            decimal TotalSavings = 0;

            foreach (ProductOrder ProdOrder in Order.ProductOrders)
            {
                TotalSavings += ProdOrder.DiscountSum;
                Builder.AppendLine(
                    string.Format(
                        "• {0} (x{1}) - {2:C2} each at {3:P} off",
                        ProdOrder.Product.Title,
                        ProdOrder.Quantity,
                        ProdOrder.ProductPrice,
                        ProdOrder.Discount));
            }
            Builder.AppendLine();
            Builder.AppendLine(string.Format("Total savings: {0:C2}", TotalSavings));
            Builder.AppendLine(string.Format("Tax: {0:C2}", Order.Total - Order.Subtotal));
            Builder.AppendLine(string.Format("Subtotal: {0:C2}", Order.Subtotal));
            Builder.Append(string.Format("Total: {0:C2}", Order.Total));
        }

        static void FormatHtmlOrder(StringBuilder Builder, Order Order)
        {
            //TODO: Replace me with a template technology such as Razor pages.
            Builder.Append("<html>");
            Builder.Append("<body>");
            Builder.Append("<h1>Order Reciept</h1>");
            Builder.Append(string.Format("<h3>Order Id: {0}</h3>", Order.Id));
            Builder.Append(string.Format("<h3>Order Time: {0}</h3>", Order.Time.ToLocalTime()));
            Builder.Append("<h3>Products Ordered</h3>");
            Builder.Append("<table style=\"width:30%\">");
            Builder.Append("<tr><th>Name</th><th>Product Price</th><th>Quantity</th><th>Discount</th><th>Sum</th></tr>");
            decimal TotalSavings = 0;
            foreach (ProductOrder ProdOrder in Order.ProductOrders)
            {
                TotalSavings += ProdOrder.DiscountSum;
                Builder.Append(
                    string.Format(
                        "<tr><td>{0}</td><td>{1:C2}</td><td>{2}</td><td>{3:P}</td><td>{4:C2}</td></tr>",
                        ProdOrder.Product.Title,
                        ProdOrder.ProductPrice,
                        ProdOrder.Quantity,
                        ProdOrder.Discount,
                        ProdOrder.Total));
            }
            Builder.Append("</table>");
            Builder.Append(
                string.Format("<h3>Total savings: {0:C2}</h3>", TotalSavings));
            Builder.Append(
                string.Format("<h3>Tax: {0:C2}</h3>", Order.Total - Order.Subtotal));
            Builder.Append(
                string.Format("<h3>Subtotal: {0:C2}</h3>", Order.Subtotal));
            Builder.Append(
                string.Format("<h3>Total: {0:C2}</h3>", Order.Total));
            Builder.Append("</body>");
            Builder.Append("</html>");
        }
    }
}
