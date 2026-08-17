using AkilliSera_API.Models;
using AkilliSera_API.Services;
using Microsoft.AspNetCore.Mvc;

namespace AkilliSera_API.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class KullaniciController : ControllerBase
    {
        private readonly DataBaseService _databaseService;

        public KullaniciController(DataBaseService databaseService)
        {
            _databaseService = databaseService;
        }

        
        [HttpGet("liste")]
        public IActionResult GetKullanicilar()
        {
            var veriler = _databaseService.KullanicilariGetir();
            return Ok(veriler);
        }

       
        [HttpPost("kayit-ol")]
        public IActionResult KayitOl([FromBody] Kullanicilar yeniKullanici)
        {
            if (yeniKullanici == null)
            {
                return BadRequest("Kullanıcı bilgileri boş olamaz.");
            }

            
            bool sonuc = _databaseService.KullaniciKaydet(yeniKullanici);

            if (!sonuc)
            {
                return BadRequest("Bu e-posta adresi ile zaten bir kayıt mevcut.");
            }

            return Ok(new { message = "Kayıt işlemi başarıyla gerçekleşti." });
        }


        [HttpPost("giris-yap")]
        public IActionResult GirisYap([FromBody] KullaniciGirisModel girisModel)
        {
            if (girisModel == null || string.IsNullOrEmpty(girisModel.Eposta) || string.IsNullOrEmpty(girisModel.Sifre))
            {
                return BadRequest("E-posta ve şifre alanları boş bırakılamaz.");
            }

            var kullanici = _databaseService.KullaniciDogrula(girisModel.Eposta, girisModel.Sifre);

            if (kullanici == null)
            {
                return Unauthorized(new { message = "E-posta veya şifre hatalı." });
            }

            
            return Ok(new { message = "Giriş başarılı.", isim = kullanici.Isim });
        }
      
        public class KullaniciGirisModel
        {
            public string Eposta { get; set; } = string.Empty;
            public string Sifre { get; set; } = string.Empty;
        }
    }
}

