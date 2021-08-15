using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Orders.Models
{
    public class ProductOrder : DbItem
    {
        public Guid OrderId { get; set; }

        public Guid ProductId { get; set; }

        public decimal ProductPrice { get; set; }

        public Product Product { get; set; }

        public decimal Discount { get; set; }

        public uint Quantity { get; set; }
        // uint as negative quantities are n/a

        public decimal DiscountSum => Discount * ProductPrice * Quantity;

        public decimal Total
        {
            get
            {
                return (1 - Discount) * ProductPrice * Quantity;
            }
        }
    }
}
