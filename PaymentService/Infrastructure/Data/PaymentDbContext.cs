using Microsoft.EntityFrameworkCore;
using PaymentService.Application.Common.Interfaces;

namespace PaymentService.Infrastructure.Data
{
    public class PaymentDbContext : DbContext, IPaymentDbContext
    {
        public PaymentDbContext(DbContextOptions<PaymentDbContext> options)
            : base(options)
        {
        }

        public DbSet<PaymentService.Domain.Entities.Payment> Payments => Set<PaymentService.Domain.Entities.Payment>();

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => base.SaveChangesAsync(cancellationToken);
    }
}
