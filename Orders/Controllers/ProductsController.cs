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
    [Route("/products")]
    public class ProductsController : ControllerBase
    {
        private readonly ILogger<ProductsController> _logger;

        IProductStorage context { get; set; }

        public ProductsController(ILogger<ProductsController> logger, IProductStorage context)
        {
            _logger = logger;
            this.context = context;
        }


        [HttpGet("{productId}")]
        public async Task<ActionResult<Product>> GetProduct(Guid productId)
        {
            return await context.Get(productId) switch
            {
                null => NotFound(),
                Product product => product
            };
        }

        [HttpPut]
        public async Task<ActionResult<Guid>> PutProduct(ExternalProductReq req)
        {
            Product product =
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = req.Title,
                    Description = req.Description,
                    Price = req.Price
                };
            return await context.Insert(product);
        }

        [HttpPost]
        public async Task<ActionResult> PostProduct(Product product)
        {
            await context.Upsert(product);
            return Ok();
        }

        [HttpDelete("{productId}")]
        public async Task<ActionResult> DeleteProduct(Guid productId)
        {
            if (await context.Delete(productId))
            {
                return Ok();
            }
            else
            {
                return NotFound();
            }
        }
    }
}
