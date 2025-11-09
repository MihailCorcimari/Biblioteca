
using System;
using System.ComponentModel.DataAnnotations;
using Biblioteca.Models;

namespace Biblioteca.Models.ReservationViewModels
{
    public class ReservationInputModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Livro")]
        public int BookId { get; set; }

        [Required]
        [Display(Name = "Leitor")]
        public int ReaderId { get; set; }

        [Required(ErrorMessage = "A data de início é obrigatória.")]
        [DataType(DataType.Date)]
        [Display(Name = "Data de início")]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "A data de fim é obrigatória.")]
        [DataType(DataType.Date)]
        [Display(Name = "Data de fim")]
        public DateTime EndDate { get; set; } = DateTime.Today.AddDays(7);

        [Display(Name = "Estado")]
        public ReservationStatus Status { get; set; } = ReservationStatus.Pending;

        [StringLength(500)]
        [Display(Name = "Notas")]
        public string? Notes { get; set; }
    }
}