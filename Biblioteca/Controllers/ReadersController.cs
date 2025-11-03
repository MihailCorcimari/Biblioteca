using Biblioteca.Models;
using Biblioteca.Models.ReaderViewModels;
using Biblioteca.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.PortableExecutable;
using System.Threading.Tasks;

namespace Biblioteca.Controllers
{
    [Authorize]
    [Route("Leitores")]
    public class ReadersController : Controller
    {
        private readonly IReaderRepository _readerRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReadersController(IReaderRepository readerRepository, UserManager<ApplicationUser> userManager)
        {
            _readerRepository = readerRepository;
            _userManager = userManager;
        }

        [HttpGet("")]
        [Authorize(Roles = RoleNames.Administrator + "," + RoleNames.Staff)]
        public async Task<IActionResult> Index()
        {
            var readers = await _readerRepository.GetAllWithUsersAsync();
            return View(readers);
        }

        [HttpGet("Detalhes/{id}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reader = await _readerRepository.GetByIdAsync(id.Value);
            if (reader == null)
            {
                return NotFound();
            }

            if (!await CanAccessReaderAsync(reader))
            {
                return Forbid();
            }

            var model = MapToInputModel(reader);
            return View(model);
        }

        [HttpGet("Editar/{id}")]
        [Authorize(Roles = RoleNames.Administrator + "," + RoleNames.Staff)]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reader = await _readerRepository.GetByIdAsync(id.Value);
            if (reader == null)
            {
                return NotFound();
            }

            ViewData["StatusMessage"] = TempData["StatusMessage"];
            var model = MapToInputModel(reader);
            return View(model);
        }

        [HttpPost("Editar/{id}")]
        [Authorize(Roles = RoleNames.Administrator + "," + RoleNames.Staff)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ReaderInputModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                ViewData["StatusMessage"] = TempData["StatusMessage"];
                return View(model);
            }

            var reader = await _readerRepository.GetByIdAsync(id);
            if (reader == null)
            {
                return NotFound();
            }

            UpdateReaderFromModel(reader, model);
            await _readerRepository.UpdateAsync(reader);

            TempData["StatusMessage"] = "Dados do leitor atualizados com sucesso.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        [HttpGet("Perfil")]
        public async Task<IActionResult> Profile()
        {
            var reader = await GetCurrentReaderAsync();
            if (reader == null)
            {
                return NotFound();
            }

            ViewData["StatusMessage"] = TempData["StatusMessage"];
            var model = MapToInputModel(reader);
            return View(model);
        }

        [HttpPost("Perfil")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ReaderInputModel model)
        {
            var reader = await GetCurrentReaderAsync();
            if (reader == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                ViewData["StatusMessage"] = TempData["StatusMessage"];
                return View(model);
            }

            UpdateReaderFromModel(reader, model);
            await _readerRepository.UpdateAsync(reader);

            TempData["StatusMessage"] = "O seu perfil foi atualizado.";
            return RedirectToAction(nameof(Profile));
        }

        [HttpGet("Apagar/{id}")]
        [Authorize(Roles = RoleNames.Administrator)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reader = await _readerRepository.GetByIdAsync(id.Value);
            if (reader == null)
            {
                return NotFound();
            }

            return View(reader);
        }

        [HttpPost("Apagar/{id}")]
        [Authorize(Roles = RoleNames.Administrator)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _readerRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> CanAccessReaderAsync(Reader reader)
        {
            if (User.IsInRole(RoleNames.Administrator) || User.IsInRole(RoleNames.Staff))
            {
                return true;
            }

            var currentUser = await _userManager.GetUserAsync(User);
            return currentUser != null && reader.ApplicationUserId == currentUser.Id;
        }

        private ReaderInputModel MapToInputModel(Reader reader)
        {
            return new ReaderInputModel
            {
                Id = reader.Id,
                FullName = reader.FullName,
                PhoneNumber = reader.PhoneNumber,
                Email = reader.ApplicationUser?.Email ?? string.Empty,
                BirthDate = reader.BirthDate,
                ProfileImageUrl = reader.ProfileImageUrl,
                ReaderCode = reader.ReaderCode
            };
        }

        private void UpdateReaderFromModel(Reader reader, ReaderInputModel model)
        {
            reader.FullName = model.FullName;
            reader.PhoneNumber = model.PhoneNumber;
            reader.BirthDate = model.BirthDate;
            reader.ProfileImageUrl = model.ProfileImageUrl;
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
    }
}