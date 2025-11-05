using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Biblioteca.Models
{
    public class ApplicationUser : IdentityUser
    {
        public Reader? ReaderProfile { get; set; }

        [StringLength(200)]
        public string? FullName { get; set; }

        public bool MustChangePassword { get; set; }
    }
}
