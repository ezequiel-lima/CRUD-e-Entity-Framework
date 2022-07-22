using Blog.Data;
using Blog.Extensions;
using Blog.Models;
using Blog.Services;
using Blog.ViewModels;
using Blog.ViewModels.Accounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureIdentity.Password;
using System.Text.RegularExpressions;

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

        // Registrar um usuario
        [HttpPost("v1/accounts")]
        public async Task<IActionResult> Post(
            [FromBody] RegisterViewModel model, 
            [FromServices] EmailService emailService,
            [FromServices] BlogDataContext context)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ResultViewModel<string>(ModelState.GetErros()));

            var user = new User
            {
                Name = model.Name,
                Email = model.Email,
                Slug = model.Email.Replace("@", "-").Replace(".", "-")
            };

            // Gero uma senha forte
            // Exemplo: G2B296X$D%1HF9B8!FB@M1}^5

            // Armazenar as senhas
            // Encriptar a senha antes de armazenar

            // *PACKAGE* SecureIdentity *By: Andre Balta*

            var password = PasswordGenerator.Generate(25);
            user.PasswordHash = PasswordHasher.Hash(password);

            try
            {
                await context.Users.AddAsync(user);
                await context.SaveChangesAsync();

                emailService.Send(
                    user.Name, 
                    user.Email, 
                    subject:"Bem vindo ao blog!",
                    body:$"Sua senha é <strong>{password}</strong>");

                return Ok(new ResultViewModel<dynamic>(new
                {
                    user = user.Email, 
                    password,
                    role
                }));
            }
            catch (DbUpdateException)
            {
                return StatusCode(400, new ResultViewModel<string>("05X99 - Este E-mail já está cadastrado"));
            }
            catch (Exception)
            {
                return StatusCode(500, new ResultViewModel<string>("05X04 - Falha interna no servidor"));
            }
        }

        [HttpPost("v1/accounts/login")]
        public async Task<IActionResult> Login(
            [FromBody] LoginViewModel model,
            [FromServices] BlogDataContext context,
            [FromServices] TokenService tokenService)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ResultViewModel<string>(ModelState.GetErros()));

            // Recuperar o usuario do banco 
            var user = await context
                .Users
                .AsNoTracking()
                .Include(x => x.Roles)
                .FirstOrDefaultAsync(x => x.Email == model.Email);

            if (user == null)
                return StatusCode(401, new ResultViewModel<string>("Usuário ou senha inválido"));

            // Compara as senhas
            if (!PasswordHasher.Verify(user.PasswordHash, model.Password))
                return StatusCode(401, new ResultViewModel<string>("Usuário ou senha inválido"));

            try
            {
                var token = tokenService.GenerateToken(user);
                return Ok(new ResultViewModel<string>(token, null));
            }
            catch (Exception)
            {
                return StatusCode(500, new ResultViewModel<string>("05X04 - Falha interna no servidor"));
            }
        }

        [Authorize]
        [HttpPost("v1/accounts/upload-image")]
        public async Task<IActionResult> UploadImage(
            [FromBody] UploadImageViewModel model,
            [FromServices] BlogDataContext context)
        {
            // Gera um nome para o arquivo para nunca gerar um nome igual, isso tambem evita cache
            var fileName = $"{Guid.NewGuid().ToString()}.jpg";
            // Vai receber a imagem do corpo de requisição 
            // E essa imagem vem sempre com o inico data:image;base64
            // Regex esta sendo utilizado para remover 
            var data = new Regex(@"^data:image\/[a-z]+;base64,").Replace(model.Base64Image, "");
            var bytes = Convert.FromBase64String(data);

            try
            {
                await System.IO.File.WriteAllBytesAsync($"wwwroot/images/{fileName}", bytes);
            }
            catch (Exception)
            {
                return StatusCode(500, new ResultViewModel<string>("05X04 - Falha interna no servidor"));
            }

            var user = await context
                .Users
                .FirstOrDefaultAsync(x => x.Email == User.Identity.Name);

            if (user == null)
                return NotFound(new ResultViewModel<User>("Usuário não encontrado"));

            user.Image = $"https://localhost:0000/images/{fileName}";

            try
            {
                context.Users.Update(user);
                await context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return StatusCode(500, new ResultViewModel<string>("05X04 - Falha interna no servidor"));
            }

            return Ok(new ResultViewModel<string>("Imagem alterada com sucesso!", null));
        }
    }
}
