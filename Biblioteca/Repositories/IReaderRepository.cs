using Biblioteca.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Biblioteca.Repositories
{
    public interface IReaderRepository : IRepository<Reader>
    {
        Task<Reader?> GetByUserIdAsync(string userId);
        Task<IEnumerable<Reader>> GetAllWithUsersAsync();
    }
}