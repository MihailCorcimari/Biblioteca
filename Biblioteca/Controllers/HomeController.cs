using Biblioteca.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Biblioteca.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var model = ErrorViewModel.ForError(
                "Ocorreu um erro inesperado",
                "Lamentamos, mas não conseguimos concluir o seu pedido. Tente novamente mais tarde.",
                statusCode: 500,
                requestId: Activity.Current?.Id ?? HttpContext.TraceIdentifier);

            return View("~/Views/Shared/Error.cshtml", model);
        }
    }
}
