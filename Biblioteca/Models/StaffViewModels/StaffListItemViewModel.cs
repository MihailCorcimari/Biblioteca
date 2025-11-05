namespace Biblioteca.Models.StaffViewModels
{
    public class StaffListItemViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool MustChangePassword { get; set; }
    }
}
