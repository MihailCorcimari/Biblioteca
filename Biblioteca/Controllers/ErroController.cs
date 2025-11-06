using System.Diagnostics;
using Biblioteca.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Biblioteca.Controllers
{
    [Route("Erro")]
    public class ErroController : Controller
    {
        private readonly ILogger<ErroController> _logger;

        public ErroController(ILogger<ErroController> logger)
        {
            _logger = logger;
        }

        [Route("")]
        public IActionResult Index()
        {
            var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerFeature>();
            if (exceptionFeature != null)
            {
                _logger.LogError(exceptionFeature.Error, "Erro não tratado durante o processamento do pedido.");
            }

            var model = ErrorViewModel.ForError(
                "Ocorreu um erro inesperado",
                "Lamentamos, mas não conseguimos concluir o seu pedido. Tente novamente mais tarde.",
                statusCode: 500,
                requestId: Activity.Current?.Id ?? HttpContext.TraceIdentifier);

            return View("~/Views/Shared/Error.cshtml", model);
        }

        [Route("Status/{statusCode}")]
        public IActionResult Status(int statusCode)
        {
            ErrorViewModel model;
            string viewPath = "~/Views/Shared/Error.cshtml";

            switch (statusCode)
            {
                case 404:
                    model = ErrorViewModel.ForError(
                        "Página não encontrada",
                        "Não foi possível encontrar o recurso solicitado. Verifique o endereço e tente novamente.",
                        statusCode: statusCode);
                    viewPath = "~/Views/Shared/NotFound.cshtml";
                    break;
                case 403:
                    model = ErrorViewModel.ForError(
                        "Acesso negado",
                        "Não tem permissões para aceder a este recurso.",
                        statusCode: statusCode);
                    break;
                case 401:
                    model = ErrorViewModel.ForError(
                        "Autenticação necessária",
                        "Inicie sessão para continuar.",
                        statusCode: statusCode);
                    break;
                default:
                    model = ErrorViewModel.ForError(
                        "Ocorreu um erro",
                        "Encontrámos um problema ao processar o seu pedido.",
                        statusCode: statusCode);
                    break;
            }

            return View(viewPath, model);
        }

        [Route("Conflito")]
        public IActionResult Conflict(string? message = null, string? returnUrl = null)
        {
            var model = ErrorViewModel.ForError(
                "Não foi possível concluir a operação",
                message ?? "A operação não pôde ser concluída porque existem dados relacionados que impedem a alteração.",
                statusCode: 409);

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                ViewData["ReturnUrl"] = returnUrl;
            }

            return View("~/Views/Shared/Conflict.cshtml", model);
        }
    }
}