using AkilliSera_API.Services;
using Microsoft.AspNetCore.Mvc;

namespace AkilliSera_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BitkiEvreController : ControllerBase
    {
        private readonly DataBaseService _databaseService;

        public BitkiEvreController(DataBaseService databaseService)
        {
            _databaseService = databaseService;
        }

        // GET: api/bitkievre/liste
        [HttpGet("liste")]
        public IActionResult GetBitkiEvreleri()
        {
            var veriler = _databaseService.BitkiEvreleriniGetir();
            return Ok(veriler);
        }
    }
} 

