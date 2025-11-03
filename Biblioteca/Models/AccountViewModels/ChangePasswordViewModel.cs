using System.ComponentModel.DataAnnotations;

namespace Biblioteca.Models.AccountViewModels
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "A password atual é obrigatória.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password atual")]
        public string OldPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "A nova password é obrigatória.")]
        [StringLength(100, ErrorMessage = "A {0} deve ter pelo menos {2} e no máximo {1} caracteres.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Nova password")]
        public string NewPassword { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar nova password")]
        [Compare("NewPassword", ErrorMessage = "A nova password e a confirmação não coincidem.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}