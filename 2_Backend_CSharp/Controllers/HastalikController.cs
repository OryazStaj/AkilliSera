using AkilliSera_API.Services;
using Microsoft.AspNetCore.Mvc;

namespace AkilliSera_API.Controllers
{
    
    
        [ApiController]
        [Route("api/[controller]")]
        public class HastalikController : ControllerBase
        {
            private readonly DataBaseService _databaseService;

            public HastalikController(DataBaseService databaseService)
            {
                _databaseService = databaseService;
            }

            [HttpGet("liste")]
            public IActionResult GetHastaliklar()
            {
                var veriler = _databaseService.BitkiHastaliklariniGetir();
                return Ok(veriler);
            }
        }
    }

