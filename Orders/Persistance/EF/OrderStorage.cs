using Microsoft.EntityFrameworkCore;
using Orders.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orders.Persistance.EF
{
    public class OrderStorage : Interfaces.IOrderStorage
    {
        private readonly DataContext _context;

        public OrderStorage(DataContext context)
        {
            _context = context;
        }

        public async Task<bool> Delete(Guid orderId)
        {
            if(await _context.Orders.FindAsync(orderId) is Order order && order != null)
            {
                await _context.SaveChangesAsync();
                _context.Orders.Remove(order);
                return true;
            } else
            {
                return false;
            }
        }

        public async Task<Order?> Get(Guid orderId)
        {
            return await _context.Orders
                .Include(order => order.ProductOrders)
                .ThenInclude(productOrder => productOrder.Product)
                .FirstOrDefaultAsync(order => order.Id == orderId);
        }

        public async Task Insert(Order order)
        {
            switch (await _context.Orders.FindAsync(order.Id))
            {
                case null:
                    _context.Orders.Add(order);
                    break;
                case Order _:
                    throw new ArgumentException("Order already exists.");
                default:
                    throw new InvalidOperationException();
            }
            await _context.SaveChangesAsync();
        }

        public async Task Upsert(Order order)
        {
            switch (await _context.Orders.FindAsync(order.Id))
            {
                case null:
                    _context.Orders.Add(order);
                    break;
                case Order original:
                    _context.Entry(original).CurrentValues.SetValues(order);
                    break;
                default:
                    throw new InvalidOperationException();
            }
            await _context.SaveChangesAsync();
        }
    }
}
