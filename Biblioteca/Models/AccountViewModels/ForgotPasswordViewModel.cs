using System.ComponentModel.DataAnnotations;

namespace Biblioteca.Models.AccountViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "O email é obrigatório.")]
        [EmailAddress(ErrorMessage = "Email inválido.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;
    }
}