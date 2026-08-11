using Microsoft.AspNetCore.Mvc;
using AkilliSera_API.Models;
using AkilliSera_API.Services;

namespace AkilliSera_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SensorsController:ControllerBase
    {
       
            private readonly DataBaseService _databaseService;

            public SensorsController(DataBaseService databaseService)
            {
                _databaseService = databaseService;
            }
        //Frontend'in geçmiş sensör verilerini çekmesi için
        [HttpGet("history")]
        public IActionResult GetHistory([FromQuery] int limit = 50)
        {
            var veriler = _databaseService.SensorGecmisi()
                                          .Take(limit)
                                          .ToList();
            return Ok(veriler);
        }
        [HttpPost("save")]
        public IActionResult SaveData([FromBody] SensorLoglari yeniVeri)
        {
            if (yeniVeri == null)
            {
                return BadRequest("Geçersiz veya boş veri.");
            }

            _databaseService.Verikaydet(yeniVeri);
            return Ok(new { message = "Sensör verisi başarıyla kaydedildi." });
        }
    }
}
