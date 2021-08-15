using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orders.Models
{
    public class ProductDiscount : DbItem
    {
        public Guid ProductId { get; set; }
        public uint Threshold { get; set; }
        public decimal Discount { get; set; }
    }
}
