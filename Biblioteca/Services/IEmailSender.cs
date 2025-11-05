using System.Threading.Tasks;

namespace Biblioteca.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string htmlMessage);
    }
}
