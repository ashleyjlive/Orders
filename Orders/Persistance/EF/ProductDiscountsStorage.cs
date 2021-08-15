using Microsoft.EntityFrameworkCore;
using Orders.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orders.Persistance.EF
{
    public class ProductDiscountStorage : Interfaces.IProductDiscountsStorage
    {
        private readonly DataContext _context;

        public ProductDiscountStorage(DataContext context)
        {
            _context = context;
        }

        public async Task<bool> Delete(Guid productId)
        {
            switch (await Get(productId))
            {
                case null:
                    return false;
                case ProductDiscount obj:
                    _context.ProductDiscounts.Remove(obj);
                    await _context.SaveChangesAsync();
                    return true;
                default:
                    throw new InvalidOperationException();
            }
        }

        public async Task<ProductDiscount> Get(Guid productId)
        {
            return await _context.ProductDiscounts.FirstOrDefaultAsync(product => product.ProductId == productId);
        }

        public async Task Insert(ProductDiscount discount)
        {
            switch (await Get(discount.ProductId))
            {
                case null:
                    _context.ProductDiscounts.Add(discount);
                    break;
                case ProductDiscount _:
                    throw new ArgumentException("Product discount already exists.");
                default:
                    throw new InvalidOperationException();
            }
            await _context.SaveChangesAsync();
        }

        public async Task Upsert(ProductDiscount discount)
        {
            switch (await Get(discount.ProductId))
            {
                case null:
                    _context.ProductDiscounts.Add(discount);
                    break;
                case ProductDiscount obj:
                    _context.Entry(obj).CurrentValues.SetValues(discount);
                    break;
                default:
                    throw new InvalidOperationException();
            }
            await _context.SaveChangesAsync();
        }
    }
}
