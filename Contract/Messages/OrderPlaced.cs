using System;
using System.Collections.Generic;
using System.Text;

namespace Contract.Messages
{
    public class OrderPlaced
    {
        public int OrderId { get; set; }
        public double Amount { get; set; }
        public string? CorrelationId { get; set; }
    }
}
