using Biblioteca.Models;
using Biblioteca.Models.StaffViewModels;
using Biblioteca.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Biblioteca.Controllers
{
    [Authorize(Roles = RoleNames.Administrator)]
    [Route("Funcionarios")]
    public class StaffController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<StaffController> _logger;

        public StaffController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IEmailSender emailSender,
            ILogger<StaffController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _emailSender = emailSender;
            _logger = logger;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            await EnsureRoleExistsAsync();
            var staffMembers = await _userManager.GetUsersInRoleAsync(RoleNames.Staff);
            var model = staffMembers
                .Select(user => new StaffListItemViewModel
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    FullName = user.FullName,
                    PhoneNumber = user.PhoneNumber,
                    EmailConfirmed = user.EmailConfirmed,
                    MustChangePassword = user.MustChangePassword
                })
                .OrderBy(user => user.FullName)
                .ThenBy(user => user.Email)
                .ToList();

            ViewData["StatusMessage"] = TempData["StatusMessage"];
            return View(model);
        }

        [HttpGet("Criar")]
        public IActionResult Create()
        {
            return View(new StaffInputModel());
        }

        [HttpPost("Criar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StaffInputModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            await EnsureRoleExistsAsync();

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError(nameof(model.Email), "Já existe um utilizador com este email.");
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                PhoneNumber = model.PhoneNumber,
                EmailConfirmed = true,
                MustChangePassword = true
            };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                foreach (var error in createResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(model);
            }

            await _userManager.AddToRoleAsync(user, RoleNames.Staff);
            await SendPasswordSetupEmailAsync(user);

            TempData["StatusMessage"] = "Funcionário criado e convite enviado.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("Editar/{id}")]
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null || !await _userManager.IsInRoleAsync(user, RoleNames.Staff))
            {
                return NotFound();
            }

            var model = new StaffInputModel
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName ?? string.Empty,
                PhoneNumber = user.PhoneNumber
            };

            return View(model);
        }

        [HttpPost("Editar/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, StaffInputModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null || !await _userManager.IsInRoleAsync(user, RoleNames.Staff))
            {
                return NotFound();
            }

            if (!string.Equals(user.Email, model.Email, StringComparison.OrdinalIgnoreCase))
            {
                var existing = await _userManager.FindByEmailAsync(model.Email);
                if (existing != null && existing.Id != user.Id)
                {
                    ModelState.AddModelError(nameof(model.Email), "Já existe outro utilizador com este email.");
                    return View(model);
                }

                user.UserName = model.Email;
                user.Email = model.Email;
            }

            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(model);
            }

            TempData["StatusMessage"] = "Dados do funcionário atualizados.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("Apagar/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null || !await _userManager.IsInRoleAsync(user, RoleNames.Staff))
            {
                return NotFound();
            }

            var model = new StaffInputModel
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName ?? string.Empty,
                PhoneNumber = user.PhoneNumber
            };

            return View(model);
        }

        [HttpPost("Apagar/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null || !await _userManager.IsInRoleAsync(user, RoleNames.Staff))
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                TempData["StatusMessage"] = string.Join(" ", result.Errors.Select(e => e.Description));
            }
            else
            {
                TempData["StatusMessage"] = "Funcionário removido.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost("ReenviarConvite/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendInvitation(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null || !await _userManager.IsInRoleAsync(user, RoleNames.Staff))
            {
                return NotFound();
            }

            user.MustChangePassword = true;
            await _userManager.UpdateAsync(user);
            await SendPasswordSetupEmailAsync(user);

            TempData["StatusMessage"] = "Novo convite enviado.";
            return RedirectToAction(nameof(Index));
        }

        private async Task EnsureRoleExistsAsync()
        {
            if (!await _roleManager.RoleExistsAsync(RoleNames.Staff))
            {
                await _roleManager.CreateAsync(new IdentityRole(RoleNames.Staff));
            }
        }

        private async Task SendPasswordSetupEmailAsync(ApplicationUser user)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = Url.Action("ResetPassword", "Account", new { token, email = user.Email }, Request.Scheme);
            if (callbackUrl == null)
            {
                _logger.LogWarning("Não foi possível gerar o link de definição de password para o utilizador {UserId}.", user.Id);
                return;
            }

            await _emailSender.SendEmailAsync(
                user.Email!,
                "Definição de palavra-passe",
                $"Foi-lhe criada uma conta de funcionário na Biblioteca. Defina a sua palavra-passe <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>aqui</a>.");
        }
    }
}
