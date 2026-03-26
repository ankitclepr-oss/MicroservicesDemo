using Microsoft.EntityFrameworkCore;
using OrderService.Application.Queries.GetOrders;
using PaymentService.Application.Common.Interfaces;

namespace OrderService.Application.Handler.GetOrders
{
    public class GetPaymentsHandler
    {
        private readonly IPaymentDbContext _db;

        public GetPaymentsHandler(IPaymentDbContext db)
        {
            _db = db;
        }

        public async Task<List<PaymentService.Domain.Entities.Payment>> Handle(GetPaymentsQuery query)
        {
            return await _db.Payments.ToListAsync();
        }
    }
}
