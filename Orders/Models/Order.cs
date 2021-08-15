using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Orders.Models
{
    /// <summary>
    /// Represents a customers order.
    /// </summary>
    public class Order : DbItem
    {
        public DateTime Time { get; set; } = DateTime.UtcNow;

        public List<ProductOrder> ProductOrders { get; set; } = new();

        public decimal Tax { get; set; } = Data.Constants.Tax;

        public decimal Subtotal => ProductOrders.Sum(ProdOrder=> ProdOrder.Total);

        public decimal Total => Subtotal * (1 + Tax);
    }
}
