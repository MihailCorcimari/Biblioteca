namespace Biblioteca.Models
{
    public class ErrorViewModel
    {
        public string Title { get; set; } = "Ocorreu um erro";
        public string Message { get; set; } = "Encontrámos um problema ao processar o seu pedido.";
        public string? RequestId { get; set; }
        public int? StatusCode { get; set; }
        public string? Details { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
        public static ErrorViewModel ForError(string title, string message, int? statusCode = null, string? requestId = null, string? details = null)
        {
            return new ErrorViewModel
            {
                Title = title,
                Message = message,
                StatusCode = statusCode,
                RequestId = requestId,
                Details = details
            };
        }
    }
}
