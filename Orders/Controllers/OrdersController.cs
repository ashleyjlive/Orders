using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orders.Persistance.Interfaces;
using Orders.Models;

namespace Orders.Controllers
{
    [ApiController]
    [Route("/orders")]
    public class OrdersController : ControllerBase
    {
        private readonly ILogger<OrdersController> _logger;

        IOrderStorage orderContext { get; set; }
        IProductStorage productContext { get; set; }
        IProductDiscountsStorage discountsContext { get; set; }

        public OrdersController(
            ILogger<OrdersController> logger, 
            IOrderStorage orderContext, 
            IProductStorage productContext, 
            IProductDiscountsStorage discountsContext)
        {
            _logger = logger;
            this.orderContext = orderContext;
            this.productContext = productContext;
            this.discountsContext = discountsContext;
        }

        [HttpGet("{orderId}")]
        public async Task<ActionResult<Order>> GetOrder(Guid orderId)
        {
            switch(await orderContext.Get(orderId))
            {
                case null:
                    return NotFound();
                case Order order:
                    return order;
            }
        }

        [HttpPut]
        public async Task<ActionResult<Guid>> PutOrder(Models.External.ExternalOrderReq extOrder)
        {
            // Now we build an internal order object here.
            Order order = new() { Id = Guid.NewGuid() };
            await AppendProductOrders(order, extOrder.ProductOrders);
            await orderContext.Insert(order);
            return order.Id;
        }

        [HttpDelete("{orderId}")]
        public async Task<ActionResult> DeleteOrder(Guid orderId)
        {
            if(await orderContext.Delete(orderId))
            {
                return Ok();
            } else
            {
                return NotFound();
            }
        }

        async Task AppendProductOrders(Order order, Dictionary<Guid, uint> Orders)
        {
            foreach(KeyValuePair<Guid, uint> KV in Orders)
            {
                switch(await productContext.Get(KV.Key))
                {
                    case Product prod:
                        ProductOrder prodOrder = new()
                        {
                            OrderId = order.Id,
                            ProductId = prod.Id,
                            ProductPrice = prod.Price,
                            Quantity = KV.Value,
                            Discount = await DetermineDiscount(prod.Id, KV.Value)
                        };
                        order.ProductOrders.Add(prodOrder);
                        break;
                    case null:
                        throw new ArgumentNullException(
                            string.Format("The requested product {0} is missing.", KV.Key));
                }
            }
        }

        async Task<decimal> DetermineDiscount(Guid productId, uint quantity)
        {
            switch(await discountsContext.Get(productId))
            {
                case null:
                    return 0;
                case ProductDiscount discount:
                    if(quantity >= discount.Threshold)
                    {
                        return discount.Discount;
                    } else
                    {
                        return 0;
                    }
            }
        }
    }
}
