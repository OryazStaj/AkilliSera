using AkilliSera_API.Services;
using Microsoft.AspNetCore.Mvc;

namespace AkilliSera_API.Controllers
{
    
        [ApiController]
        [Route("api/[controller]")]
        public class BildirimController : ControllerBase
        {
            private readonly DataBaseService _databaseService;

            public BildirimController(DataBaseService databaseService)
            {
                _databaseService = databaseService;
            }

            // GET: api/bildirim/liste
            [HttpGet("liste")]
            public IActionResult GetBildirimler()
            {
                var veriler = _databaseService.BildirimleriGetir();
                return Ok(veriler);
            }
        }
    }

