using System;
using System.ComponentModel.DataAnnotations;

namespace Biblioteca.Models.ReaderViewModels
{
    public class ReaderInputModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Nome completo")]
        [StringLength(200)]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "Telemóvel")]
        [Phone]
        public string? PhoneNumber { get; set; }

        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Data de nascimento")]
        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }

        [Display(Name = "Foto de perfil (URL)")]
        [StringLength(512)]
        public string? ProfileImageUrl { get; set; }

        [Display(Name = "Código de leitor")]
        public string ReaderCode { get; set; } = string.Empty;
    }
}