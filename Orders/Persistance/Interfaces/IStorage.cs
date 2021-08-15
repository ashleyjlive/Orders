using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orders.Persistance.Interfaces
{
    public interface IStorage
    {
        IOrderStorage Orders { get; set; }
        IProductStorage Products { get; set; }
        
        IProductDiscountsStorage ProductDiscounts { get; set; }
    }
}
