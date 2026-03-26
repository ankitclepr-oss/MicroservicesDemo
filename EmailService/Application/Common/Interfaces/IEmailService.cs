using EmailService.Domain.Entities;

namespace EmailService.Application.Common.Interfaces
{
    public interface IEmailService
    {
        Task SendAsync(Email email);
    }
}
