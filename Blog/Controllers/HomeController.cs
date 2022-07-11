using Microsoft.AspNetCore.Mvc;

// Health Check
namespace Blog.Controllers
{
    [ApiController]
    [Route("")]
    public class HomeController: ControllerBase
    {
        [HttpGet("")]
        public IActionResult Get()
        {
            return Ok();
        }
    }
}
