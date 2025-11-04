
using Biblioteca.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Biblioteca.Repositories
{
    public interface IReservationRepository : IRepository<Reservation>
    {
        Task<IEnumerable<Reservation>> GetAllWithDetailsAsync();
        Task<IEnumerable<Reservation>> GetByReaderIdAsync(int readerId);
        Task<Reservation?> GetByIdWithDetailsAsync(int id);
    }
}