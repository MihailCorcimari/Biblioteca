using Biblioteca.Data;
using Biblioteca.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Biblioteca.Repositories
{
    public class ReservationRepository : Repository<Reservation>, IReservationRepository
    {
        public ReservationRepository(LibraryContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Reservation>> GetAllWithDetailsAsync()
        {
            return await _dbSet
                .Include(r => r.Book)
                .Include(r => r.Reader)
                    .ThenInclude(reader => reader!.ApplicationUser)
                .OrderByDescending(r => r.ReservedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Reservation>> GetByReaderIdAsync(int readerId)
        {
            return await _dbSet
                .Include(r => r.Book)
                .Where(r => r.ReaderId == readerId)
                .OrderByDescending(r => r.ReservedAt)
                .ToListAsync();
        }

        public async Task<Reservation?> GetByIdWithDetailsAsync(int id)
        {
            return await _dbSet
                .Include(r => r.Book)
                .Include(r => r.Reader)
                    .ThenInclude(reader => reader!.ApplicationUser)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public override async Task<Reservation?> GetByIdAsync(int id)
        {
            return await GetByIdWithDetailsAsync(id);
        }
    }
}