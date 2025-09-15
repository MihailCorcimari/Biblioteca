using Biblioteca.Data;
using Biblioteca.Models;

namespace Biblioteca.Repositories
{
    public class BookRepository : Repository<Book>, IBookRepository
    {
        public BookRepository(LibraryContext context) : base(context)
        {
        }
    }
}
