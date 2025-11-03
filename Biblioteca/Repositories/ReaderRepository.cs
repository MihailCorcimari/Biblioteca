using Biblioteca.Data;
using Biblioteca.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Biblioteca.Repositories
{
    public class ReaderRepository : Repository<Reader>, IReaderRepository
    {
        public ReaderRepository(LibraryContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Reader>> GetAllWithUsersAsync()
        {
            return await _dbSet
                .Include(r => r.ApplicationUser)
                .OrderBy(r => r.FullName)
                .ToListAsync();
        }

        public override async Task<Reader?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(r => r.ApplicationUser)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<Reader?> GetByUserIdAsync(string userId)
        {
            return await _dbSet
                .Include(r => r.ApplicationUser)
                .FirstOrDefaultAsync(r => r.ApplicationUserId == userId);
        }
    }
}