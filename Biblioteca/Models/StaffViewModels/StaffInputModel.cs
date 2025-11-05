using System.ComponentModel.DataAnnotations;

namespace Biblioteca.Models.StaffViewModels
{
    public class StaffInputModel
    {
        public string? Id { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Nome completo")]
        [StringLength(200)]
        public string FullName { get; set; } = string.Empty;

        [Phone]
        [Display(Name = "Telemóvel")]
        public string? PhoneNumber { get; set; }
    }
}