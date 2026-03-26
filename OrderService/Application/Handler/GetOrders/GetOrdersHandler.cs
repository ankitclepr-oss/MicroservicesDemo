using Microsoft.EntityFrameworkCore;
using OrderService.Application.Common.Interfaces;
using OrderService.Application.Queries.GetOrders;
using OrderService.Domain.Entities;

namespace OrderService.Application.Handler.GetOrders
{
    public class GetOrdersHandler
    {
        private readonly IOrderDbContext _db;

        public GetOrdersHandler(IOrderDbContext db)
        {
            _db = db;
        }

        public async Task<List<Order>> Handle(GetOrdersQuery query)
        {
            return await _db.Orders.ToListAsync();
        }
    }
}
