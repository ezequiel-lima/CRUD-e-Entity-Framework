using Blog.Data;
using Blog.Models;
using Blog.ViewModels;
using Blog.ViewModels.Posts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blog.Controllers
{
    [ApiController]
    public class PostController : ControllerBase
    {
        [HttpGet("v1/posts")]
        public async Task<IActionResult> GetAsync(
            [FromServices] BlogDataContext context, 
            [FromQuery] int skip = 0, // skip = page
            [FromQuery] int take = 25) // take = pageSize
        {
            var count = await context.Posts.AsNoTracking().CountAsync();
            var posts = await context
                .Posts
                .AsNoTracking()
                .Include(x => x.Category)
                .Include(x => x.Author)
                .Select(x => new ListPostsViewModel
                {
                    Id = x.Id,
                    Title = x.Title,
                    Slug = x.Slug,
                    LastUpdateDate = x.LastUpdateDate,
                    Category = x.Category.Name,
                    Author = $"{x.Author.Name} - {x.Author.Email}"
                })
                .Skip(skip * take)
                .Take(take)
                .ToListAsync();

            return Ok(new ResultViewModel<dynamic>(new
            {
                total = count,
                skip,
                take,
                posts
            }));
        }

        [HttpGet("v1/post/{id:int}")]
        public async Task<IActionResult> DetailsAsync(
            [FromServices] BlogDataContext context,
            [FromRoute] int id)
        {
            try
            {
                var post = await context
                    .Posts
                    .AsNoTracking()
                    .Include(x => x.Author)
                    .ThenInclude(x => x.Roles)
                    .Include(x => x.Category)
                    .Select(x => new ListPostsViewModel
                    {
                        Id = x.Id,
                        Title = x.Title,
                        Slug = x.Slug,
                        LastUpdateDate = x.LastUpdateDate,
                        Category = x.Category.Name,
                        Author = $"{x.Author.Name} - {x.Author.Email}"
                    })
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (post == null)
                    return NotFound(new ResultViewModel<Post>("Conteúdo não encontrado"));

                return Ok(new ResultViewModel<dynamic>(new
                {
                    post
                }));
            }
            catch 
            {
                return StatusCode(500, new ResultViewModel<Post>("05X04 - Falha interna no servidor"));
            }
        }

        // Listar todos os posts de uma determinada categoria
        [HttpGet("v1/posts/category/{category}")]
        public async Task<IActionResult> GetByCategoryAsync(
            [FromRoute] string category,
            [FromServices] BlogDataContext context,
            [FromRoute] int skip = 0,
            [FromRoute] int take = 25)
        {
            try
            {
                var count = await context.Posts.AsNoTracking().CountAsync();
                var posts = await context
                    .Posts
                    .AsNoTracking()
                    .Include(x => x.Author)
                    .Include(x => x.Category)
                    .Where(x => x.Category.Slug == category)
                    .Select(x => new ListPostsViewModel
                    {
                        Id = x.Id,
                        Title = x.Title,
                        Slug = x.Slug,
                        LastUpdateDate = x.LastUpdateDate,
                        Category = x.Category.Name,
                        Author = $"{x.Author.Name} - {x.Author.Email}"
                    })
                    .Skip(skip * take)
                    .Take(take)
                    .OrderByDescending(x => x.LastUpdateDate)
                    .ToListAsync();

                return Ok(new ResultViewModel<dynamic>(new
                {
                    total = count,
                    skip,
                    take,
                    posts
                }));
            }
            catch
            {
                return StatusCode(500, new ResultViewModel<Post>("05X04 - Falha interna no servidor"));
            }
        }
    }
}
