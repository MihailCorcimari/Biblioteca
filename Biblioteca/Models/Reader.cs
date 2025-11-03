using System;
using System.ComponentModel.DataAnnotations;

namespace Biblioteca.Models
{
    public class Reader
    {
        public int Id { get; set; }

        [Required]
        public string ApplicationUserId { get; set; } = string.Empty;

        public ApplicationUser? ApplicationUser { get; set; }

        [Required]
        [Display(Name = "Nome completo")]
        [StringLength(200)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Código de leitor")]
        [StringLength(16)]
        public string ReaderCode { get; set; } = string.Empty;

        [Phone]
        [Display(Name = "Telemóvel")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Data de nascimento")]
        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }

        [Display(Name = "Foto de perfil (URL)")]
        [StringLength(512)]
        public string? ProfileImageUrl { get; set; }

        [Display(Name = "Data de criação")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public static string GenerateReaderCode()
        {
            var randomPart = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpperInvariant();
            return $"LE-{randomPart}";
        }
    }
}