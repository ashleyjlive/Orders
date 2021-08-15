using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orders.Models
{
    public class Product : DbItem
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
    }
}
