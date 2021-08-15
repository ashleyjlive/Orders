using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orders.Models.External
{
    public class ExternalOrderReq
    {
        public Dictionary<Guid, uint> ProductOrders { get; set; } = new();
    }
}
