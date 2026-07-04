using HalisahaApp.API.Data;
using HalisahaApp.API.DTOs;
using HalisahaApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HalisahaApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // JWT ile koruma altında
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _db;

        public AdminController(AppDbContext db)
        {
            _db = db;
        }

        // GET: api/admin/rezervasyonlar?sahaId=1&tarih=2025-05-10
        [HttpGet("rezervasyonlar")]
        public async Task<IActionResult> Rezervasyonlar([FromQuery] int sahaId, [FromQuery] DateTime? tarih)
        {
            var query = _db.Rezervasyonlar
                .Where(r => r.SahaId == sahaId)
                .AsQueryable();

            if (tarih.HasValue)
                query = query.Where(r => r.Tarih.Date == tarih.Value.Date);

            var liste = await query
                .OrderBy(r => r.Tarih)
                .ThenBy(r => r.BaslangicSaati)
                .Select(r => new
                {
                    r.Id,
                    r.AdSoyad,
                    r.Email,
                    r.Telefon,
                    r.TarihVeSaat,
                    r.ToplamUcret,
                    OdemeDurumu = r.OdemeDurumu.ToString(),
                    Durum = r.Durum.ToString(),
                    r.OlusturmaTarihi
                })
                .ToListAsync();

            return Ok(liste);
        }

        // POST: api/admin/manuel-rezervasyon
        // Admin ödemesiz manuel rezervasyon ekler
        [HttpPost("manuel-rezervasyon")]
        public async Task<IActionResult> ManuelRezervasyonEkle([FromBody] ManuelRezervasyonDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            bool doluMu = await _db.Rezervasyonlar.AnyAsync(r =>
                r.SahaId == dto.SahaId &&
                r.Tarih.Date == dto.Tarih.Date &&
                r.BaslangicSaati == dto.BaslangicSaati &&
                r.Durum == RezervasyonDurumu.Aktif);

            if (doluMu) return Conflict("Bu saat dolu.");

            var rezervasyon = new Rezervasyon
            {
                SahaId = dto.SahaId,
                AdSoyad = dto.AdSoyad,
                Email = dto.Email ?? "manuel@admin.com",
                Telefon = dto.Telefon ?? "-",
                Tarih = dto.Tarih.Date,
                BaslangicSaati = dto.BaslangicSaati,
                BitisSaati = dto.BaslangicSaati + 1,
                ToplamUcret = 0, // Manuel ekleme ücretsiz
                OdemeDurumu = OdemeDurumu.Odendi, // Admin ekledi = onaylı
                Durum = RezervasyonDurumu.Aktif
            };

            _db.Rezervasyonlar.Add(rezervasyon);
            await _db.SaveChangesAsync();

            return Ok(new { Mesaj = "Manuel rezervasyon eklendi.", Id = rezervasyon.Id });
        }

        // DELETE: api/admin/iptal/5
        // Sadece admin iptal edebilir
        [HttpDelete("iptal/{id}")]
        public async Task<IActionResult> IptalEt(int id, [FromBody] IptalDto dto)
        {
            var rezervasyon = await _db.Rezervasyonlar.FindAsync(id);
            if (rezervasyon == null) return NotFound();
            if (rezervasyon.Durum == RezervasyonDurumu.Iptal) return BadRequest("Zaten iptal edilmiş.");

            rezervasyon.Durum = RezervasyonDurumu.Iptal;
            rezervasyon.IptalTarihi = DateTime.Now;
            rezervasyon.IptalNedeni = dto.Neden;

            // TODO: Ödeme iadesi (iyzico refund)
            // Ödeme yapılmışsa iade başlat

            await _db.SaveChangesAsync();
            return Ok(new { Mesaj = "Rezervasyon iptal edildi." });
        }

        // GET: api/admin/istatistik?sahaId=1
        [HttpGet("istatistik")]
        public async Task<IActionResult> Istatistik([FromQuery] int sahaId)
        {
            var bugun = DateTime.Today;

            var bugunRezervasyonlar = await _db.Rezervasyonlar
                .Where(r => r.SahaId == sahaId && r.Tarih.Date == bugun && r.Durum == RezervasyonDurumu.Aktif)
                .CountAsync();

            var bugunGelir = await _db.Rezervasyonlar
                .Where(r => r.SahaId == sahaId && r.Tarih.Date == bugun
                         && r.Durum == RezervasyonDurumu.Aktif
                         && r.OdemeDurumu == OdemeDurumu.Odendi)
                .SumAsync(r => r.ToplamUcret);

            var haftaGelir = await _db.Rezervasyonlar
                .Where(r => r.SahaId == sahaId
                         && r.Tarih >= bugun.AddDays(-7)
                         && r.OdemeDurumu == OdemeDurumu.Odendi)
                .SumAsync(r => r.ToplamUcret);

            return Ok(new { BugunRezervasyonSayisi = bugunRezervasyonlar, BugunGelir = bugunGelir, HaftaGelir = haftaGelir });
        }

        // PUT: api/admin/saha-ayarlari/1
        [HttpPut("saha-ayarlari/{sahaId}")]
        public async Task<IActionResult> SahaAyarlariGuncelle(int sahaId, [FromBody] SahaAyarlariDto dto)
        {
            var saha = await _db.Sahalar.FindAsync(sahaId);
            if (saha == null) return NotFound();

            saha.Ad = dto.Ad;
            saha.AcilisSaati = dto.AcilisSaati;
            saha.KapanisSaati = dto.KapanisSaati;
            saha.SaatlikUcret = dto.SaatlikUcret;

            await _db.SaveChangesAsync();
            return Ok(new { Mesaj = "Ayarlar güncellendi." });
        }
        // POST: api/admin/saha-ekle
        [HttpPost("saha-ekle")]
        public async Task<IActionResult> SahaEkle([FromBody] SahaEkleDto dto)
        {
            // Admin'in mevcut saha sayısını bul (URL kodu için)
            var mevcutSayisi = await _db.Sahalar.CountAsync(s => s.AdminId == dto.AdminId);
            var yeniSaha = new Models.Saha
            {
                Ad = dto.Ad,
                SahaKodu = $"saha-{Guid.NewGuid().ToString().Substring(0, 8)}",
                AdminId = dto.AdminId,
                AcilisSaati = 17,
                KapanisSaati = 24,
                SaatlikUcret = dto.SaatlikUcret,
                Aktif = true
            };
            _db.Sahalar.Add(yeniSaha);
            await _db.SaveChangesAsync();
            return Ok(new { Mesaj = "Saha eklendi.", SahaId = yeniSaha.Id });
        }

        // DELETE: api/admin/saha-sil/5
        [HttpDelete("saha-sil/{sahaId}")]
        public async Task<IActionResult> SahaSil(int sahaId)
        {
            var saha = await _db.Sahalar.FindAsync(sahaId);
            if (saha == null) return NotFound();

            // Tamamen silmek yerine pasif yap (rezervasyon geçmişini korumak için)
            saha.Aktif = false;
            await _db.SaveChangesAsync();
            return Ok(new { Mesaj = "Saha kaldırıldı." });
        }
    }
}
