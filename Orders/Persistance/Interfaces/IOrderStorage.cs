using Orders.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orders.Persistance.Interfaces
{
    /// <summary>
    /// Defines the interface methods for CRUD operations in order storage.
    /// </summary>
    public interface IOrderStorage
    {
        Task<Order> Get(Guid orderId);

        Task Insert(Order order);

        Task Upsert(Order order);

        Task<bool> Delete(Guid orderId);
    }
}
