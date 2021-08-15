using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orders.Persistance
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<Models.Order> Orders { get; set; }
        public DbSet<Models.Product> Products { get; set; }
        public DbSet<Models.ProductOrder> ProductOrders { get; set; }
        public DbSet<Models.ProductDiscount> ProductDiscounts { get; set; }
    }
}
