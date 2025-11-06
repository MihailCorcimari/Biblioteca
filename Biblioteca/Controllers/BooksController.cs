using System.Linq;
using Biblioteca.Models;
using Biblioteca.Models.BookViewModels;
using Biblioteca.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Controllers
{
    [Route("Livros")]
    public class BooksController : Controller
    {
        private readonly IBookRepository _repository;

        public BooksController(IBookRepository repository)
        {
            _repository = repository;
        }

        // GET: Livros
        [AllowAnonymous]
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var books = await _repository.GetAllAsync();
            var viewModel = books
                .Select(book => BookAvailabilityViewModel.FromBook(book))
                .ToList();
            return View(viewModel);
        }

        // GET: Livros/Detalhes/5
        [AllowAnonymous]
        [HttpGet("Detalhes/{id}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var book = await _repository.GetByIdAsync(id.Value);
            if (book == null)
            {
                return NotFound();
            }
            var viewModel = BookAvailabilityViewModel.FromBook(book);
            return View(viewModel);
        }

        // GET: Livros/Criar
        [Authorize(Roles = RoleNames.Administrator + "," + RoleNames.Staff)]
        [HttpGet("Criar")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Livros/Criar
        [Authorize(Roles = RoleNames.Administrator + "," + RoleNames.Staff)]
        [HttpPost("Criar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Author,PublicationDate")] Book book)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _repository.AddAsync(book);
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    var message = "Não foi possível criar o livro porque já existe um registo com dados incompatíveis.";
                    var returnUrl = Url.Action(nameof(Create));
                    return RedirectToAction("Conflict", "Erro", new { message, returnUrl });
                }
            }
            return View(book);
        }

        // GET: Livros/Editar/5
        [Authorize(Roles = RoleNames.Administrator + "," + RoleNames.Staff)]
        [HttpGet("Editar/{id}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var book = await _repository.GetByIdAsync(id.Value);
            if (book == null)
            {
                return NotFound();
            }
            return View(book);
        }

        // POST: Livros/Editar/5
        [Authorize(Roles = RoleNames.Administrator + "," + RoleNames.Staff)]
        [HttpPost("Editar/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Author,PublicationDate")] Book book)
        {
            if (id != book.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _repository.UpdateAsync(book);
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    var message = "Não foi possível atualizar o livro porque existem conflitos com outros registos.";
                    var returnUrl = Url.Action(nameof(Edit), new { id = book.Id });
                    return RedirectToAction("Conflict", "Erro", new { message, returnUrl });
                }
            }
            return View(book);
        }

        // GET: Livros/Apagar/5
        [Authorize(Roles = RoleNames.Administrator + "," + RoleNames.Staff)]
        [HttpGet("Apagar/{id}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var book = await _repository.GetByIdAsync(id.Value);
            if (book == null)
            {
                return NotFound();
            }
            return View(book);
        }

        // POST: Livros/Apagar/5
        [Authorize(Roles = RoleNames.Administrator + "," + RoleNames.Staff)]
        [HttpPost("Apagar/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _repository.DeleteAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                var message = "Não foi possível eliminar o livro porque existem dados relacionados.";
                var returnUrl = Url.Action(nameof(Delete), new { id });
                return RedirectToAction("Conflict", "Erro", new { message, returnUrl });
            }
        }
    }
}