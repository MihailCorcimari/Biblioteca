using Biblioteca.Data;
using Biblioteca.Models;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Repositories
{
    public class BookRepository : Repository<Book>, IBookRepository
    {
        public BookRepository(LibraryContext context) : base(context)
        {
        }
        public override async Task<IEnumerable<Book>> GetAllAsync()
        {
            return await _context.Books
                .Include(b => b.Reservations)
                .ToListAsync();
        }

        public override async Task<Book?> GetByIdAsync(int id)
        {
            return await _context.Books
                .Include(b => b.Reservations)
                .FirstOrDefaultAsync(b => b.Id == id);
        }
    }
}
