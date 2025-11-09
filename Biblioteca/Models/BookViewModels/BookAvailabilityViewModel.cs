using System;
using System.Linq;

using Biblioteca.Models;

namespace Biblioteca.Models.BookViewModels
{
    public class BookAvailabilityViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public DateTime? PublicationDate { get; set; }
        public bool IsAvailable { get; set; }
        public DateTime? CurrentReservationEndDate { get; set; }
        public DateTime? NextReservationStartDate { get; set; }
        public bool HasOpenEndedReservation { get; set; }
        public string AvailabilitySummary { get; set; } = string.Empty;

        public static BookAvailabilityViewModel FromBook(Book book, DateTime? referenceDate = null)
        {
            if (book == null)
            {
                throw new ArgumentNullException(nameof(book));
            }

            var today = (referenceDate ?? DateTime.UtcNow).Date;

            var activeReservations = book.Reservations
                .Where(r => r.Status == ReservationStatus.Pending
                    || r.Status == ReservationStatus.Confirmed
                    || r.Status == ReservationStatus.Collected)
                .OrderBy(r => r.StartDate)
                .ToList();

            var currentReservation = activeReservations
                .FirstOrDefault(r => r.StartDate.Date <= today
                    && r.EndDate.Date >= today);

            var nextReservation = activeReservations
                .FirstOrDefault(r => r.StartDate.Date > today);

            var availabilitySummary = "Disponível";
            var isAvailable = currentReservation == null;
            var currentReservationEnd = currentReservation?.EndDate.Date;
            var hasOpenEndedReservation = false;

            if (!isAvailable)
            {
                availabilitySummary = currentReservationEnd.HasValue
                    ? $"Reservado até {currentReservationEnd.Value.ToShortDateString()}"
                     : "Reservado";
            }
            else if (nextReservation != null)
            {
                availabilitySummary = $"Disponível (reservado a partir de {nextReservation.StartDate.ToShortDateString()})";
            }

            return new BookAvailabilityViewModel
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                PublicationDate = book.PublicationDate,
                IsAvailable = isAvailable,
                CurrentReservationEndDate = currentReservationEnd,
                NextReservationStartDate = nextReservation?.StartDate.Date,
                HasOpenEndedReservation = hasOpenEndedReservation,
                AvailabilitySummary = availabilitySummary
            };
        }
    }
}