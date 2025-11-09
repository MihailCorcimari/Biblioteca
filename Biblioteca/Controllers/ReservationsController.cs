using Biblioteca.Models;
using Biblioteca.Models.ReservationViewModels;
using Biblioteca.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Biblioteca.Controllers
{
    [Authorize(Roles = RoleNames.Administrator + "," + RoleNames.Staff + "," + RoleNames.Reader)]
    [Route("Reservas")]
    public class ReservationsController : Controller
    {
        private readonly IReservationRepository _reservationRepository;
        private readonly IBookRepository _bookRepository;
        private readonly IReaderRepository _readerRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<ReservationsController> _logger;

        public ReservationsController(
            IReservationRepository reservationRepository,
            IBookRepository bookRepository,
            IReaderRepository readerRepository,
            UserManager<ApplicationUser> userManager,
            IEmailSender emailSender,
            ILogger<ReservationsController> logger)
        {
            _reservationRepository = reservationRepository;
            _bookRepository = bookRepository;
            _readerRepository = readerRepository;
            _userManager = userManager;
            _emailSender = emailSender;
            _logger = logger;
        }

        [HttpGet("")]
        [Authorize(Roles = RoleNames.Administrator + "," + RoleNames.Staff)]
        public async Task<IActionResult> Index()
        {
            var reservations = await _reservationRepository.GetAllWithDetailsAsync();
            return View(reservations);
        }

        [HttpGet("Minhas")]
        [Authorize(Roles = RoleNames.Reader)]
        public async Task<IActionResult> My()
        {
            var reader = await GetCurrentReaderAsync();
            if (reader == null)
            {
                return RedirectToAction("Profile", "Readers");
            }

            var reservations = await _reservationRepository.GetByReaderIdAsync(reader.Id);
            ViewData["ReaderName"] = reader.FullName;
            return View(reservations);
        }

        [HttpGet("Detalhes/{id}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _reservationRepository.GetByIdWithDetailsAsync(id.Value);
            if (reservation == null)
            {
                return NotFound();
            }

            if (!await CanAccessReservationAsync(reservation))
            {
                return Forbid();
            }

            return View(reservation);
        }

        [HttpGet("Criar")]
        [Authorize(Roles = RoleNames.Administrator + "," + RoleNames.Staff)]
        public async Task<IActionResult> Create()
        {
            await LoadSelectionListsAsync();
            var model = new ReservationInputModel
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(7)
            };
            return View(model);
        }

        [HttpPost("Criar")]
        [Authorize(Roles = RoleNames.Administrator + "," + RoleNames.Staff)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReservationInputModel model)
        {
            await LoadSelectionListsAsync();

            if (ModelState.IsValid && !ValidateDateRange(model))
            {
                ModelState.AddModelError(nameof(model.EndDate), "A data de fim deve ser posterior ou igual à data de início.");
            }

            if (ModelState.IsValid && !await ValidateAvailabilityAsync(model))
            {
                ModelState.AddModelError(string.Empty, "Já existe uma reserva para este livro nas datas selecionadas.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var reservation = new Reservation
            {
                BookId = model.BookId,
                ReaderId = model.ReaderId,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                Status = model.Status,
                Notes = model.Notes,
                ReservedAt = DateTime.UtcNow
            };

            try
            {
                await _reservationRepository.AddAsync(reservation);
                var reservationDetails = await _reservationRepository.GetByIdWithDetailsAsync(reservation.Id);
                if (reservationDetails != null)
                {
                    await NotifyStaffAboutReservationAsync(reservationDetails, "criada");
                }
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                var message = "Não foi possível criar a reserva porque existem conflitos com outros registos.";
                var returnUrl = Url.Action(nameof(Create));
                return RedirectToAction("Conflict", "Erro", new { message, returnUrl });
            }
        }

        [HttpGet("Editar/{id}")]
        [Authorize(Roles = RoleNames.Administrator + "," + RoleNames.Staff)]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _reservationRepository.GetByIdWithDetailsAsync(id.Value);
            if (reservation == null)
            {
                return NotFound();
            }

            var model = MapToInputModel(reservation);
            await LoadSelectionListsAsync();
            return View(model);
        }

        [HttpPost("Editar/{id}")]
        [Authorize(Roles = RoleNames.Administrator + "," + RoleNames.Staff)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ReservationInputModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }
            await LoadSelectionListsAsync();

            if (ModelState.IsValid && !ValidateDateRange(model))
            {
                ModelState.AddModelError(nameof(model.EndDate), "A data de fim deve ser posterior ou igual à data de início.");
            }

            if (ModelState.IsValid && !await ValidateAvailabilityAsync(model, id))
            {
                ModelState.AddModelError(string.Empty, "Já existe uma reserva para este livro nas datas selecionadas.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var reservation = await _reservationRepository.GetByIdWithDetailsAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }

            reservation.BookId = model.BookId;
            reservation.ReaderId = model.ReaderId;
            reservation.StartDate = model.StartDate;
            reservation.EndDate = model.EndDate;
            reservation.Status = model.Status;
            reservation.Notes = model.Notes;

            try
            {
                await _reservationRepository.UpdateAsync(reservation);
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                var message = "Não foi possível atualizar a reserva porque existem conflitos com outros registos.";
                var returnUrl = Url.Action(nameof(Edit), new { id });
                return RedirectToAction("Conflict", "Erro", new { message, returnUrl });
            }
        }

        [HttpGet("Apagar/{id}")]
        [Authorize(Roles = RoleNames.Administrator)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _reservationRepository.GetByIdWithDetailsAsync(id.Value);
            if (reservation == null)
            {
                return NotFound();
            }

            return View(reservation);
        }

        [HttpPost("Apagar/{id}")]
        [Authorize(Roles = RoleNames.Administrator)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _reservationRepository.DeleteAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                var message = "Não foi possível eliminar a reserva porque existem dados relacionados.";
                var returnUrl = Url.Action(nameof(Delete), new { id });
                return RedirectToAction("Conflict", "Erro", new { message, returnUrl });
            }
        }

        [HttpGet("Nova")]
        [Authorize(Roles = RoleNames.Reader)]
        public async Task<IActionResult> CreateForReader()
        {
            var reader = await GetCurrentReaderAsync();
            if (reader == null)
            {
                return RedirectToAction("Profile", "Readers");
            }

            await LoadSelectionListsAsync(includeReaders: false);

            var model = new ReservationInputModel
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(7)
            };

            return View("CreateForReader", model);
        }

        [HttpPost("Nova")]
        [Authorize(Roles = RoleNames.Reader)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateForReader(ReservationInputModel model)
        {
            var reader = await GetCurrentReaderAsync();
            if (reader == null)
            {
                return RedirectToAction("Profile", "Readers");
            }

            ModelState.Remove(nameof(model.ReaderId));
            ModelState.Remove(nameof(model.Status));
            model.ReaderId = reader.Id;
            model.Status = ReservationStatus.Pending;

            if (ModelState.IsValid && !ValidateDateRange(model))
            {
                ModelState.AddModelError(nameof(model.EndDate), "A data de fim deve ser posterior ou igual à data de início.");
            }

            if (ModelState.IsValid && !await ValidateAvailabilityAsync(model))
            {
                ModelState.AddModelError(string.Empty, "Já existe uma reserva para este livro nas datas selecionadas.");
            }

            if (!ModelState.IsValid)
            {
                await LoadSelectionListsAsync(includeReaders: false);
                return View("CreateForReader", model);
            }

            var reservation = new Reservation
            {
                BookId = model.BookId,
                ReaderId = reader.Id,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                Status = ReservationStatus.Pending,
                Notes = model.Notes,
                ReservedAt = DateTime.UtcNow
            };

            try
            {
                await _reservationRepository.AddAsync(reservation);
                var reservationDetails = await _reservationRepository.GetByIdWithDetailsAsync(reservation.Id);
                if (reservationDetails != null)
                {
                    await NotifyStaffAboutReservationAsync(reservationDetails, "criada");
                }
                return RedirectToAction(nameof(My));
            }
            catch (DbUpdateException)
            {
                var message = "Não foi possível criar a reserva porque existem conflitos com outros registos.";
                var returnUrl = Url.Action(nameof(CreateForReader));
                return RedirectToAction("Conflict", "Erro", new { message, returnUrl });
            }
        }

        [HttpPost("Cancelar/{id}")]
        [Authorize(Roles = RoleNames.Reader)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var reservation = await _reservationRepository.GetByIdWithDetailsAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }

            var reader = await GetCurrentReaderAsync();
            if (reader == null || reservation.ReaderId != reader.Id)
            {
                return Forbid();
            }

            if (reservation.Status == ReservationStatus.Cancelled)
            {
                return RedirectToAction(nameof(My));
            }

            reservation.Status = ReservationStatus.Cancelled;
            try
            {
                await _reservationRepository.UpdateAsync(reservation);
                await NotifyStaffAboutReservationAsync(reservation, "cancelada");
                return RedirectToAction(nameof(My));
            }
            catch (DbUpdateException)
            {
                var message = "Não foi possível cancelar a reserva devido a um conflito de dados.";
                var returnUrl = Url.Action(nameof(My));
                return RedirectToAction("Conflict", "Erro", new { message, returnUrl });
            }
        }

        private ReservationInputModel MapToInputModel(Reservation reservation)
        {
            return new ReservationInputModel
            {
                Id = reservation.Id,
                BookId = reservation.BookId,
                ReaderId = reservation.ReaderId,
                StartDate = reservation.StartDate,
                EndDate = reservation.EndDate,
                Status = reservation.Status,
                Notes = reservation.Notes
            };
        }

        private bool ValidateDateRange(ReservationInputModel model)
        {
            return model.EndDate.Date >= model.StartDate.Date;
        }

        private async Task LoadSelectionListsAsync(bool includeReaders = true)
        {
            var books = (await _bookRepository.GetAllAsync()).OrderBy(b => b.Title).ToList();

            ViewBag.Books = books
                .Select(b => new SelectListItem(b.Title, b.Id.ToString()))
                .ToList();

            ViewBag.HasBooks = books.Any();
            if (includeReaders)
            {
                var readers = (await _readerRepository.GetAllWithUsersAsync()).OrderBy(r => r.FullName).ToList();
                ViewBag.Readers = readers
                    .Select(r => new SelectListItem(r.FullName + " (" + (r.ApplicationUser?.Email ?? string.Empty) + ")", r.Id.ToString()))
                    .ToList();
                ViewBag.HasReaders = readers.Any();
            }
            else
            {
                ViewBag.HasReaders = true;
            }
        }

        private async Task<bool> ValidateAvailabilityAsync(ReservationInputModel model, int? reservationId = null)
        {
            return !await _reservationRepository.HasConflictingReservationAsync(
                 model.BookId,
                 model.StartDate,
                 model.EndDate,
                 reservationId);
        }

        private async Task<Reader?> GetCurrentReaderAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return null;
            }

            return await _readerRepository.GetByUserIdAsync(user.Id);
        }

        private async Task<bool> CanAccessReservationAsync(Reservation reservation)
        {
            if (User.IsInRole(RoleNames.Administrator) || User.IsInRole(RoleNames.Staff))
            {
                return true;
            }

            var reader = await GetCurrentReaderAsync();
            return reader != null && reservation.ReaderId == reader.Id;
        }
        private async Task NotifyStaffAboutReservationAsync(Reservation reservation, string action)
        {
            var staffMembers = await _userManager.GetUsersInRoleAsync(RoleNames.Staff);
            var staffEmails = staffMembers
                .Where(user => !string.IsNullOrWhiteSpace(user.Email))
                .Select(user => user.Email!)
                .Distinct()
                .ToList();

            if (!staffEmails.Any())
            {
                return;
            }

            if (reservation.Book == null)
            {
                reservation.Book = await _bookRepository.GetByIdAsync(reservation.BookId);
            }

            if (reservation.Reader == null || reservation.Reader.ApplicationUser == null)
            {
                var reservationWithDetails = await _reservationRepository.GetByIdWithDetailsAsync(reservation.Id);
                if (reservationWithDetails != null)
                {
                    reservation = reservationWithDetails;
                }
            }

            var bookTitle = reservation.Book?.Title ?? $"Livro #{reservation.BookId}";
            var readerName = reservation.Reader?.FullName ?? "Leitor desconhecido";
            var readerEmail = reservation.Reader?.ApplicationUser?.Email;
            var culture = CultureInfo.GetCultureInfo("pt-PT");
            var startDate = reservation.StartDate.ToString("dd/MM/yyyy", culture);
            var endDate = reservation.EndDate.ToString("dd/MM/yyyy", culture);
            var reservationUrl = Url.Action(nameof(Details), "Reservations", new { id = reservation.Id }, Request.Scheme);

            var encoder = HtmlEncoder.Default;
            var builder = new StringBuilder();
            builder.Append("<p>Uma reserva foi ").Append(encoder.Encode(action)).Append(" por um leitor.</p>");
            builder.Append("<ul>");
            builder.Append("<li><strong>Livro:</strong> ").Append(encoder.Encode(bookTitle)).Append("</li>");
            builder.Append("<li><strong>Leitor:</strong> ").Append(encoder.Encode(readerName));
            if (!string.IsNullOrWhiteSpace(readerEmail))
            {
                builder.Append(" (" + encoder.Encode(readerEmail!) + ")");
            }
            builder.Append("</li>");
            builder.Append("<li><strong>Período:</strong> ").Append(encoder.Encode($"{startDate} - {endDate}")).Append("</li>");
            builder.Append("<li><strong>Estado atual:</strong> ").Append(encoder.Encode(reservation.Status.ToString())).Append("</li>");
            if (!string.IsNullOrWhiteSpace(reservation.Notes))
            {
                builder.Append("<li><strong>Notas do leitor:</strong> ").Append(encoder.Encode(reservation.Notes!)).Append("</li>");
            }
            if (!string.IsNullOrWhiteSpace(reservationUrl))
            {
                builder.Append("<li><a href=\"").Append(encoder.Encode(reservationUrl!)).Append("\">Ver detalhes da reserva</a></li>");
            }
            builder.Append("</ul>");

            var subject = $"Reserva {action}: {bookTitle}";

            foreach (var email in staffEmails)
            {
                try
                {
                    await _emailSender.SendEmailAsync(email, subject, builder.ToString());
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Falha ao enviar notificação de reserva {Action} para {Email}.", action, email);
                }
            }
        }
    }
}