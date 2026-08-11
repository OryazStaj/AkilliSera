using Microsoft.AspNetCore.Mvc;
using AkilliSera_API.Models;
using AkilliSera_API.Services;

namespace AkilliSera_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IlaclamaController:ControllerBase
    {
        private readonly DataBaseService _databaseService;

        public IlaclamaController(DataBaseService databaseService)
        {
            _databaseService = databaseService;
        }
        [HttpGet("gecmis")]
        public IActionResult GetIlaclamaGecmisi()
        {
            var veriler = _databaseService.IlaclamaGecmisi();
            return Ok(veriler);
        }
        [HttpPost("ekle")]
        public IActionResult AddIlaclama([FromBody] IlaclamaTakip yeniIlaclama)
        {
            if (yeniIlaclama == null)
            {
                return BadRequest("Geçersiz veya boş veri.");
            }

            _databaseService.IlaclamaEkle(yeniIlaclama);
            return Ok(new { message = "İlaçlama kaydı başarıyla eklendi." });
        }
    }
}
