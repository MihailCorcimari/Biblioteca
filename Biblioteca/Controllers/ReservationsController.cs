using System;
using System.Linq;
using System.Threading.Tasks;
using Biblioteca.Models;
using Biblioteca.Models.ReservationViewModels;
using Biblioteca.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

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

        public ReservationsController(
            IReservationRepository reservationRepository,
            IBookRepository bookRepository,
            IReaderRepository readerRepository,
            UserManager<ApplicationUser> userManager)
        {
            _reservationRepository = reservationRepository;
            _bookRepository = bookRepository;
            _readerRepository = readerRepository;
            _userManager = userManager;
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

            if (!ValidateDateRange(model))
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

            await _reservationRepository.AddAsync(reservation);
            return RedirectToAction(nameof(Index));
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

            if (!ValidateDateRange(model))
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

            await _reservationRepository.UpdateAsync(reservation);
            return RedirectToAction(nameof(Index));
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
            await _reservationRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
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

            if (!ValidateDateRange(model))
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

            await _reservationRepository.AddAsync(reservation);
            return RedirectToAction(nameof(My));
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
            await _reservationRepository.UpdateAsync(reservation);

            return RedirectToAction(nameof(My));
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
            return !model.EndDate.HasValue || model.EndDate.Value.Date >= model.StartDate.Date;
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
            return !await _reservationRepository.HasConflictingReservationAsync(model.BookId, model.StartDate, model.EndDate, reservationId);
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
    }
}