using Orders.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orders.Persistance.Interfaces
{
    public interface IProductDiscountsStorage
    {
        Task<ProductDiscount> Get(Guid productId);

        Task Insert(ProductDiscount productDiscount);

        Task Upsert(ProductDiscount productDiscount);

        Task<bool> Delete(Guid productId);
    }
}
