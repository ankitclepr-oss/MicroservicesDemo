using Microsoft.EntityFrameworkCore;

namespace PaymentService.Application.Common.Interfaces
{
    public interface IPaymentDbContext
    {
        DbSet<PaymentService.Domain.Entities.Payment> Payments { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
