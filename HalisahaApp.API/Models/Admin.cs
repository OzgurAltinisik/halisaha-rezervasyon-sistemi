namespace HalisahaApp.API.Models
{
    public class Admin
    {
        public int Id { get; set; }

        public string AdSoyad { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        // Şifre hash'li tutulacak (BCrypt ile)
        public string SifreHash { get; set; } = string.Empty;

        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
        public DateTime? SonGirisTarihi { get; set; }

        // İlişkiler
        public ICollection<Saha> Sahalar { get; set; } = new List<Saha>();
    }
}
