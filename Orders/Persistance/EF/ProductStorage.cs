using Microsoft.EntityFrameworkCore;
using Orders.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orders.Persistance.EF
{
    public class ProductStorage : Interfaces.IProductStorage
    {
        private readonly DataContext _context;

        public ProductStorage(DataContext context)
        {
            _context = context;
        }

        public async Task<bool> Delete(Guid productId)
        {
            switch(await _context.Products.FindAsync(productId))
            {
                case null:
                    return false;
                case Product product:
                    _context.Products.Remove(product);
                    await _context.SaveChangesAsync();
                    return true;
                default:
                    throw new InvalidOperationException();
            }
        }

        public async Task<Product> Get(Guid productId)
        {
            return await _context.Products.FindAsync(productId);
        }

        public async Task<Guid> Insert(Product product)
        {
            switch (await _context.Products.FindAsync(product.Id))
            {
                case null:
                    _context.Products.Add(product);
                    break;
                case Product _:
                    throw new ArgumentException("Product already exists.");
                default:
                    throw new InvalidOperationException();
            }
            await _context.SaveChangesAsync();
            return product.Id;
        }

        public async Task Upsert(Product product)
        {
            switch (await _context.Products.FindAsync(product.Id))
            {
                case null:
                    _context.Products.Add(product);
                    break;
                case Product original:
                    _context.Entry(original).CurrentValues.SetValues(product);
                    break;
                default:
                    throw new InvalidOperationException();
            }
            await _context.SaveChangesAsync();
        }
    }
}
