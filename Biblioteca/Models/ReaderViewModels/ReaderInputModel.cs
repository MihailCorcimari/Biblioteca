using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

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

        [Display(Name = "Foto de perfil")]
        public IFormFile? ProfileImageFile { get; set; }

        [Display(Name = "Foto de perfil")]
        public string? ProfileImageDataUrl { get; set; }

        [Display(Name = "Remover foto de perfil")]
        public bool RemoveProfileImage { get; set; }

        [Display(Name = "Código de leitor")]
        public string ReaderCode { get; set; } = string.Empty;
    }
}
