using HalisahaApp.API.Models;

namespace HalisahaApp.API.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task RezervasyonOnayGonder(Rezervasyon rezervasyon)
        {
            // TODO: Gmail ayarları yapıldığında aktif edilecek
            await Task.CompletedTask;
            return;
        }
    }
}