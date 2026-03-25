using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;

namespace OrderService.Application.Common.Interfaces
{
    public interface IOrderDbContext
    {
        DbSet<Order> Orders { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
