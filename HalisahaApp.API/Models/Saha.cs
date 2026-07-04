namespace HalisahaApp.API.Models
{
    public class Saha
    {
        public int Id { get; set; }
        public string Ad { get; set; } = string.Empty;
        public string SahaKodu { get; set; } = string.Empty; // URL slug: "konyaspor-halisaha"

        // Saat ayarları
        public int AcilisSaati { get; set; } = 17;   // 17 = 17:00
        public int KapanisSaati { get; set; } = 24;  // 24 = 24:00

        // Fiyat
        public decimal SaatlikUcret { get; set; } = 3000;

        // Nakit deadline (kaç gün önce, şimdilik kullanılmıyor ama ileride lazım olabilir)
        public int NakitDeadlineGun { get; set; } = 1;

        // Durum
        public bool Aktif { get; set; } = true;
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;

        // Admin bilgisi
        public int AdminId { get; set; }
        public Admin Admin { get; set; } = null!;

        // İlişkiler
        public ICollection<Rezervasyon> Rezervasyonlar { get; set; } = new List<Rezervasyon>();
    }
}
