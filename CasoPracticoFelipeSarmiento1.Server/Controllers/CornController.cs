using Microsoft.AspNetCore.Mvc;

namespace CasoPracticoFelipeSarmiento1.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CornController : ControllerBase
    {

        private readonly RateLimiterService _rateLimiterService;

        public CornController(RateLimiterService rateLimiterService)
        {
            _rateLimiterService = rateLimiterService;
        }

        [HttpPost("purchase")]
        public async Task<IActionResult> PurchaseCorn([FromHeader] string clientId)
        {
            if (await _rateLimiterService.CanPurchaseCornAsync(clientId))
            {
                return Ok("Corn purchased successfully!");
            }
            return StatusCode(429, "Too Many Requests: Limit exceeded.");
        }
    }
}
