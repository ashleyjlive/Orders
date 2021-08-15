using Orders.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orders.Persistance.Interfaces
{
    /// <summary>
    /// Defines the interface methods for CRUD operations in product storage.
    /// </summary>
    public interface IProductStorage
    {
        Task<Product> Get(Guid productId);

        Task<Guid> Insert(Product product);

        Task Upsert(Product product);

        Task<bool> Delete(Guid productId);
    }
}
