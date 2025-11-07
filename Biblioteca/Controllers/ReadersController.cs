using Biblioteca.Models;
using Biblioteca.Models.ReaderViewModels;
using Biblioteca.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Biblioteca.Controllers
{
    [Authorize]
    [Route("Leitores")]
    public class ReadersController : Controller
    {
        private readonly IReaderRepository _readerRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private static readonly string[] _allowedImageContentTypes = new[]
        {
            "image/jpeg",
            "image/png",
            "image/gif",
            "image/webp"
        };

        private const long MaxProfileImageSizeBytes = 2 * 1024 * 1024; // 2 MB

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

            var reader = await _readerRepository.GetByIdAsync(id);
            if (reader == null)
            {
                return NotFound();
            }

            ValidateProfileImage(model);

            if (!ModelState.IsValid)
            {
                ViewData["StatusMessage"] = TempData["StatusMessage"];
                model.ProfileImageDataUrl = BuildProfileImageDataUrl(reader);
                model.Email = reader.ApplicationUser?.Email ?? string.Empty;
                model.ReaderCode = reader.ReaderCode;
                return View(model);
            }

            UpdateReaderFromModel(reader, model);
            try
            {
                await _readerRepository.UpdateAsync(reader);
            }
            catch (DbUpdateException)
            {
                var message = "Não foi possível atualizar o leitor porque existem dados em conflito.";
                var returnUrl = Url.Action(nameof(Edit), new { id });
                return RedirectToAction("Conflict", "Erro", new { message, returnUrl });
            }

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

            ValidateProfileImage(model);

            if (!ModelState.IsValid)
            {
                ViewData["StatusMessage"] = TempData["StatusMessage"];
                model.ProfileImageDataUrl = BuildProfileImageDataUrl(reader);
                model.Email = reader.ApplicationUser?.Email ?? string.Empty;
                model.ReaderCode = reader.ReaderCode;
                return View(model);
            }

            UpdateReaderFromModel(reader, model);
            try
            {
                await _readerRepository.UpdateAsync(reader);
            }
            catch (DbUpdateException)
            {
                var message = "Não foi possível atualizar o perfil porque os dados entram em conflito com outro registo.";
                var returnUrl = Url.Action(nameof(Profile));
                return RedirectToAction("Conflict", "Erro", new { message, returnUrl });
            }

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
            try
            {
                await _readerRepository.DeleteAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                var message = "Não foi possível eliminar o leitor porque existem dados associados.";
                var returnUrl = Url.Action(nameof(Delete), new { id });
                return RedirectToAction("Conflict", "Erro", new { message, returnUrl });
            }
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
                ProfileImageDataUrl = BuildProfileImageDataUrl(reader),
                ReaderCode = reader.ReaderCode
            };
        }

        private void UpdateReaderFromModel(Reader reader, ReaderInputModel model)
        {
            reader.FullName = model.FullName;
            reader.PhoneNumber = model.PhoneNumber;
            reader.BirthDate = model.BirthDate;

            if (model.RemoveProfileImage)
            {
                reader.ProfileImage = null;
                reader.ProfileImageContentType = null;
            }
            else if (model.ProfileImageFile != null && model.ProfileImageFile.Length > 0)
            {
                using var memoryStream = new MemoryStream();
                model.ProfileImageFile.CopyTo(memoryStream);
                reader.ProfileImage = memoryStream.ToArray();
                reader.ProfileImageContentType = model.ProfileImageFile.ContentType;
            }
        }

        private void ValidateProfileImage(ReaderInputModel model)
        {
            if (model.RemoveProfileImage)
            {
                return;
            }

            if (model.ProfileImageFile == null || model.ProfileImageFile.Length == 0)
            {
                return;
            }

            if (!_allowedImageContentTypes.Contains(model.ProfileImageFile.ContentType))
            {
                ModelState.AddModelError(nameof(model.ProfileImageFile), "A imagem tem de estar em formato JPEG, PNG, GIF ou WebP.");
            }

            if (model.ProfileImageFile.Length > MaxProfileImageSizeBytes)
            {
                var maxMegabytes = MaxProfileImageSizeBytes / 1024d / 1024d;
                ModelState.AddModelError(nameof(model.ProfileImageFile), $"A imagem não pode exceder {maxMegabytes:0.#} MB.");
            }
        }

        private static string? BuildProfileImageDataUrl(Reader reader)
        {
            if (reader.ProfileImage == null || reader.ProfileImage.Length == 0)
            {
                return null;
            }

            var contentType = string.IsNullOrWhiteSpace(reader.ProfileImageContentType)
                ? "image/png"
                : reader.ProfileImageContentType;

            var base64 = Convert.ToBase64String(reader.ProfileImage);
            return $"data:{contentType};base64,{base64}";
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