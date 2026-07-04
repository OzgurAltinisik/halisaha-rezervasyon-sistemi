using HalisahaApp.API.Data;
using HalisahaApp.API.DTOs;
using HalisahaApp.API.Models;
using HalisahaApp.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HalisahaApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RezervasyonController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly EmailService _emailService;

        public RezervasyonController(AppDbContext db, EmailService emailService)
        {
            _db = db;
            _emailService = emailService;
        }

        // GET: api/rezervasyon/musaitlik?sahaKodu=konyaspor&tarih=2025-05-10
        // Belirli bir gün için dolu/boş saatleri döner
        [HttpGet("musaitlik")]
        public async Task<IActionResult> Musaitlik([FromQuery] string sahaKodu, [FromQuery] DateTime tarih)
        {
            var saha = await _db.Sahalar.FirstOrDefaultAsync(s => s.SahaKodu == sahaKodu && s.Aktif);
            if (saha == null) return NotFound("Saha bulunamadı.");

            // O güne ait aktif rezervasyonlar
            var doluSaatler = await _db.Rezervasyonlar
                .Where(r => r.SahaId == saha.Id
                         && r.Tarih.Date == tarih.Date
                         && r.Durum == RezervasyonDurumu.Aktif
                         && r.OdemeDurumu == OdemeDurumu.Odendi)
                .Select(r => r.BaslangicSaati)
                .ToListAsync();

            // Saat dilimlerini oluştur (17-24 arası ya da saha ayarına göre)
            var saatler = new List<SaatDilimiDto>();
            for (int saat = saha.AcilisSaati; saat < saha.KapanisSaati; saat++)
            {
                saatler.Add(new SaatDilimiDto
                {
                    BaslangicSaati = saat,
                    BitisSaati = saat + 1,
                    Etiket = $"{saat:D2}:00 – {saat + 1:D2}:00",
                    Dolu = doluSaatler.Contains(saat),
                    Ucret = saha.SaatlikUcret
                });
            }

            return Ok(new { Saha = saha.Ad, Ucret = saha.SaatlikUcret, Saatler = saatler });
        }

        // POST: api/rezervasyon/olustur
        [HttpPost("olustur")]
        public async Task<IActionResult> Olustur([FromBody] RezervasyonOlusturDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var saha = await _db.Sahalar.FirstOrDefaultAsync(s => s.SahaKodu == dto.SahaKodu && s.Aktif);
            if (saha == null) return NotFound("Saha bulunamadı.");

            // Saat geçerlilik kontrolü
            if (dto.BaslangicSaati < saha.AcilisSaati || dto.BaslangicSaati >= saha.KapanisSaati)
                return BadRequest("Geçersiz saat aralığı.");

            // Çakışma kontrolü
            bool doluMu = await _db.Rezervasyonlar.AnyAsync(r =>
                r.SahaId == saha.Id &&
                r.Tarih.Date == dto.Tarih.Date &&
                r.BaslangicSaati == dto.BaslangicSaati &&
                r.Durum == RezervasyonDurumu.Aktif &&
                r.OdemeDurumu == OdemeDurumu.Odendi);

            if (doluMu) return Conflict("Bu saat dolu.");

            // Rezervasyonu oluştur (ödeme bekleniyor)
            var rezervasyon = new Rezervasyon
            {
                SahaId = saha.Id,
                AdSoyad = dto.AdSoyad,
                Email = dto.Email,
                Telefon = dto.Telefon,
                Tarih = dto.Tarih.Date,
                BaslangicSaati = dto.BaslangicSaati,
                BitisSaati = dto.BaslangicSaati + 1,
                ToplamUcret = saha.SaatlikUcret,
                OdemeDurumu = OdemeDurumu.Bekliyor,
                Durum = RezervasyonDurumu.Aktif
            };

            _db.Rezervasyonlar.Add(rezervasyon);
            await _db.SaveChangesAsync();

            // TODO: iyzico ödeme başlat, callback'te OdemeDurumu.Odendi yap
            // Şimdilik direkt onaylıyoruz (iyzico entegrasyonu sonraki adımda)

            return Ok(new { RezervasyonId = rezervasyon.Id, Mesaj = "Rezervasyon oluşturuldu, ödemeye yönlendiriliyorsunuz." });
        }

        // POST: api/rezervasyon/odeme-callback (iyzico webhook)
        [HttpPost("odeme-callback")]
        public async Task<IActionResult> OdemeCallback([FromBody] OdemeCallbackDto dto)
        {
            var rezervasyon = await _db.Rezervasyonlar
                .Include(r => r.Saha)
                .FirstOrDefaultAsync(r => r.Id == dto.RezervasyonId);

            if (rezervasyon == null) return NotFound();

            if (dto.Basarili)
            {
                rezervasyon.OdemeDurumu = OdemeDurumu.Odendi;
                rezervasyon.IyzicoOdemeId = dto.IyzicoOdemeId;
                await _db.SaveChangesAsync();

                // Onay e-postası gönder
                await _emailService.RezervasyonOnayGonder(rezervasyon);
                return Ok("Ödeme onaylandı.");
            }
            else
            {
                rezervasyon.OdemeDurumu = OdemeDurumu.Basarisiz;
                await _db.SaveChangesAsync();
                return Ok("Ödeme başarısız.");
            }
        }
        // GET: api/rezervasyon/sahalar?adminId=1
        [HttpGet("sahalar")]
        public async Task<IActionResult> Sahalar([FromQuery] int adminId)
        {
            var sahalar = await _db.Sahalar
                .Where(s => s.AdminId == adminId && s.Aktif)
                .Select(s => new { s.Id, s.Ad, s.SahaKodu, s.SaatlikUcret })
                .ToListAsync();
            return Ok(sahalar);
        }
    }
}