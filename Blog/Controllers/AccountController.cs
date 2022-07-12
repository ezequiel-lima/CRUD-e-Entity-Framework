using Blog.Services;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Controllers
{
    [ApiController]
    public class AccountController : ControllerBase
    {
        // Uma tecnica que fala para esse controller que ele depende de um tokenService para existir
        private readonly TokenService _tokenService;

        public AccountController(TokenService tokenService)
        {
            _tokenService = tokenService;
        }

        [HttpPost("v1/login")]
        public IActionResult Login()
        {
            // Geramos o token baseado no tokenService
            var token = _tokenService.GenerateToken(null);

            return Ok(token);
        }
    }
}
