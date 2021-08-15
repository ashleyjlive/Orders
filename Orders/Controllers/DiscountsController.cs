using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orders.Persistance.Interfaces;
using Orders.Models;
using Orders.Models.External;

namespace Orders.Controllers
{
    [ApiController]
    [Route("/discounts")]
    public class DiscountsController : ControllerBase
    {
        private readonly ILogger<ProductsController> _logger;

        IProductDiscountsStorage context { get; set; }

        public DiscountsController(ILogger<ProductsController> logger, IProductDiscountsStorage context)
        {
            _logger = logger;
            this.context = context;
        }


        [HttpGet("{productId}")]
        public async Task<ActionResult<ExternalDiscountReq>> GetDiscount(Guid productId)
        {
            return await context.Get(productId) switch
            {
                null => NotFound(),
                ProductDiscount product => 
                    new ExternalDiscountReq() 
                    { 
                        Discount = product.Discount, 
                        ProductId = product.ProductId, 
                        Threshold = product.Threshold
                    }
            };
        }

        [HttpPut]
        public async Task<ActionResult> PutDiscount(ExternalDiscountReq req)
        {
            ProductDiscount discount =
                new()
                {
                    Id = Guid.NewGuid(),
                    Discount = req.Discount,
                    ProductId = req.ProductId,
                    Threshold = req.Threshold
                };
            await context.Insert(discount);
            return Ok();
        }

        [HttpDelete("{productId}")]
        public async Task<ActionResult> DeleteDiscount(Guid productId)
        {
            if(await context.Delete(productId))
            {
                return Ok();
            } else
            {
                return NotFound();
            }
        }
    }
}
