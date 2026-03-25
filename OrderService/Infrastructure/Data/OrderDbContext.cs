using Microsoft.EntityFrameworkCore;
using OrderService.Application.Common.Interfaces;
using OrderService.Domain.Entities;

namespace OrderService.Infrastructure.Data
{
    public class OrderDbContext : DbContext, IOrderDbContext
    {
        public OrderDbContext(DbContextOptions<OrderDbContext> options)
            : base(options)
        {
        }

        public DbSet<Order> Orders => Set<Order>();

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => base.SaveChangesAsync(cancellationToken);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>().HasKey(x => x.Id);

            base.OnModelCreating(modelBuilder);
        }
    }
}
