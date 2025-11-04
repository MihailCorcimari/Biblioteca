
using System;
using System.ComponentModel.DataAnnotations;

namespace Biblioteca.Models
{
    public class Reservation
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Livro")]
        public int BookId { get; set; }

        public Book? Book { get; set; }

        [Required]
        [Display(Name = "Leitor")]
        public int ReaderId { get; set; }

        public Reader? Reader { get; set; }

        [Display(Name = "Data de reserva")]
        public DateTime ReservedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Data de início")]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Data de fim")]
        public DateTime? EndDate { get; set; }

        [Display(Name = "Estado")]
        public ReservationStatus Status { get; set; } = ReservationStatus.Pending;

        [StringLength(500)]
        [Display(Name = "Notas")]
        public string? Notes { get; set; }
    }

    public enum ReservationStatus
    {
        [Display(Name = "Pendente")]
        Pending,
        [Display(Name = "Confirmada")]
        Confirmed,
        [Display(Name = "Recolhida")]
        Collected,
        [Display(Name = "Concluída")]
        Completed,
        [Display(Name = "Cancelada")]
        Cancelled
    }
}