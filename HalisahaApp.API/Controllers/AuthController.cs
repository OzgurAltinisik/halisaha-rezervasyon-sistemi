using HalisahaApp.API.Data;
using HalisahaApp.API.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HalisahaApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;

        public AuthController(AppDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        // POST: api/auth/giris
        [HttpPost("giris")]
        public async Task<IActionResult> Giris([FromBody] AdminGirisDto dto)
        {
            var admin = await _db.Adminler
                .FirstOrDefaultAsync(a => a.Email == dto.Email);

            if (admin == null)
                return Unauthorized(new { Mesaj = "E-posta veya şifre hatalı." });

            // BCrypt ile şifre doğrulama
            bool sifreDogruMu = BCrypt.Net.BCrypt.Verify(dto.Sifre, admin.SifreHash);
            if (!sifreDogruMu)
                return Unauthorized(new { Mesaj = "E-posta veya şifre hatalı." });

            // Son giriş tarihini güncelle
            admin.SonGirisTarihi = DateTime.Now;
            await _db.SaveChangesAsync();

            // JWT token oluştur
            var token = TokenUret(admin.Id, admin.Email, admin.AdSoyad);

            // Admin'in sahalarını da döndür
            var sahalar = await _db.Sahalar
                .Where(s => s.AdminId == admin.Id)
                .Select(s => new { s.Id, s.Ad, s.SahaKodu })
                .ToListAsync();

            return Ok(new
            {
                Token = token,
                Admin = new
                {
                    admin.Id,
                    admin.AdSoyad,
                    admin.Email
                },
                Sahalar = sahalar
            });
        }

        // POST: api/auth/admin-kayit (Sadece ilk kurulumda kullan, sonra kaldır!)
        [HttpPost("admin-kayit")]
        public async Task<IActionResult> AdminKayit([FromBody] AdminKayitDto dto)
        {
            // Aynı e-posta ile kayıt var mı?
            bool mevcutMu = await _db.Adminler.AnyAsync(a => a.Email == dto.Email);
            if (mevcutMu)
                return BadRequest(new { Mesaj = "Bu e-posta zaten kayıtlı." });

            var admin = new Models.Admin
            {
                AdSoyad = dto.AdSoyad,
                Email = dto.Email,
                SifreHash = BCrypt.Net.BCrypt.HashPassword(dto.Sifre),
                OlusturmaTarihi = DateTime.Now
            };

            _db.Adminler.Add(admin);
            await _db.SaveChangesAsync();

            // Admin kaydedilince örnek bir saha da oluştur
            var saha = new Models.Saha
            {
                Ad = dto.SahaAdi,
                SahaKodu = dto.SahaKodu.ToLower().Replace(" ", "-"),
                AdminId = admin.Id,
                AcilisSaati = 17,
                KapanisSaati = 24,
                SaatlikUcret = dto.SaatlikUcret,
                Aktif = true
            };

            _db.Sahalar.Add(saha);
            await _db.SaveChangesAsync();

            return Ok(new { Mesaj = "Admin ve saha oluşturuldu.", AdminId = admin.Id, SahaId = saha.Id });
        }

        private string TokenUret(int adminId, string email, string adSoyad)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, adminId.ToString()),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Name, adSoyad),
                new Claim("AdminId", adminId.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(8), // 8 saat geçerli
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
