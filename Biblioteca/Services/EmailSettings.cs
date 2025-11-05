namespace Biblioteca.Services
{
    public class EmailSettings
    {
        public string SenderName { get; set; } = "Biblioteca";
        public string SenderEmail { get; set; } = string.Empty;
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 25;
        public bool EnableSsl { get; set; } = true;
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
