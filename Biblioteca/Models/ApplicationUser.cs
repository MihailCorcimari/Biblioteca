using Microsoft.AspNetCore.Identity;
using System.Reflection.PortableExecutable;

namespace Biblioteca.Models
{
    public class ApplicationUser : IdentityUser
    {
        public Reader? ReaderProfile { get; set; }
    }
}
