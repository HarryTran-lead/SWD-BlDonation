using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SWD_BLDONATION.DTOs;
using SWD_BLDONATION.Models.Generated;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SWD_BLDONATION.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlogPostsController : ControllerBase
    {
        private readonly BloodDonationContext _context;
        private readonly string _uploadsFolder;

        public BlogPostsController(BloodDonationContext context, IWebHostEnvironment env)
        {
            _context = context;
            _uploadsFolder = Path.Combine(env.WebRootPath, "assets", "Upload_Image");
            if (!Directory.Exists(_uploadsFolder))
                Directory.CreateDirectory(_uploadsFolder);
        }

        // GET: api/BlogPosts
        [HttpGet]
        public async Task<ActionResult<object>> GetBlogPosts([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page < 1 || pageSize < 1)
                return BadRequest(new { Message = "Invalid page or pageSize." });

            var query = _context.BlogPosts
                .Join(_context.Users,
                    p => p.UserId,
                    u => u.UserId,
                    (p, u) => new { Post = p, UserName = u.UserName });

            var posts = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new BlogPostDto
                {
                    PostId = x.Post.PostId,
                    UserId = x.Post.UserId,
                    Title = x.Post.Title,
                    Content = x.Post.Content,
                    Category = x.Post.Category,
                    CreatedAt = x.Post.CreatedAt,
                    UpdatedAt = x.Post.UpdatedAt,
                    ImgPath = x.Post.ImgPath,
                    UserName = x.UserName
                })
                .ToListAsync();

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return Ok(new
            {
                Message = "Retrieved blog posts successfully.",
                Data = new
                {
                    Posts = posts,
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    CurrentPage = page,
                    PageSize = pageSize
                }
            });
        }

        // GET: api/BlogPosts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetBlogPost(int id)
        {
            var post = await _context.BlogPosts
                .Where(p => p.PostId == id)
                .Join(_context.Users,
                    p => p.UserId,
                    u => u.UserId,
                    (p, u) => new { Post = p, UserName = u.UserName })
                .Select(x => new BlogPostDto
                {
                    PostId = x.Post.PostId,
                    UserId = x.Post.UserId,
                    Title = x.Post.Title,
                    Content = x.Post.Content,
                    Category = x.Post.Category,
                    CreatedAt = x.Post.CreatedAt,
                    UpdatedAt = x.Post.UpdatedAt,
                    ImgPath = x.Post.ImgPath,
                    UserName = x.UserName
                })
                .FirstOrDefaultAsync();

            if (post == null)
                return NotFound(new { Message = $"Blog post with id = {id} not found." });

            return Ok(new { Message = "Retrieved blog post successfully.", Data = post });
        }

        // GET: api/BlogPosts/search
        [HttpGet("search")]
        public async Task<ActionResult<object>> SearchBlogPosts([FromQuery] BlogPostSearchQueryDto query)
        {
            if (query.Page < 1 || query.PageSize < 1)
                return BadRequest(new { Message = "Invalid page or pageSize." });

            var dbQuery = _context.BlogPosts
                .Join(_context.Users,
                    p => p.UserId,
                    u => u.UserId,
                    (p, u) => new { Post = p, UserName = u.UserName });

            if (query.Id.HasValue)
                dbQuery = dbQuery.Where(x => x.Post.PostId == query.Id.Value);

            if (!string.IsNullOrEmpty(query.Title))
                dbQuery = dbQuery.Where(x => x.Post.Title.Contains(query.Title.Trim()));

            var posts = await dbQuery
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(x => new BlogPostDto
                {
                    PostId = x.Post.PostId,
                    UserId = x.Post.UserId,
                    Title = x.Post.Title,
                    Content = x.Post.Content,
                    Category = x.Post.Category,
                    CreatedAt = x.Post.CreatedAt,
                    UpdatedAt = x.Post.UpdatedAt,
                    ImgPath = x.Post.ImgPath,
                    UserName = x.UserName
                })
                .ToListAsync();

            var totalCount = await dbQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);

            return Ok(new
            {
                Message = "Search completed successfully.",
                Data = new
                {
                    Posts = posts,
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    CurrentPage = query.Page,
                    PageSize = query.PageSize
                }
            });
        }

        // POST: api/BlogPosts
        [HttpPost]
        public async Task<ActionResult<object>> CreatePost([FromForm] CreateBlogPostDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new
                {
                    Message = "Invalid data.",
                    Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });

            var user = await _context.Users.FindAsync(dto.UserId);
            if (user == null || user.IsDeleted)
                return BadRequest(new { Message = $"User with id = {dto.UserId} does not exist or is deleted." });

            var post = new BlogPost
            {
                UserId = dto.UserId,
                Title = dto.Title,
                Content = dto.Content,
                Category = dto.Category,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            if (dto.Img != null && dto.Img.Length > 0)
            {
                // Validate image format
                var allowedExtensions = new[] { ".png", ".jpg", ".jpeg", ".gif" };
                var extension = Path.GetExtension(dto.Img.FileName).ToLower();
                if (!allowedExtensions.Contains(extension))
                    return BadRequest(new { Message = "Invalid image format. Only PNG, JPG, JPEG, and GIF are allowed." });

                var uniqueFileName = Guid.NewGuid().ToString() + ".png"; 
                var filePath = Path.Combine(_uploadsFolder, uniqueFileName);

                try
                {
                    using var memoryStream = new MemoryStream();
                    await dto.Img.CopyToAsync(memoryStream);
                    using var image = Image.FromStream(memoryStream);
                    image.Save(filePath, ImageFormat.Png);
                    post.ImgPath = $"/assets/Upload_Image/{uniqueFileName}";
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { Message = "Error saving image file.", Detail = ex.Message });
                }
            }

            _context.BlogPosts.Add(post);
            await _context.SaveChangesAsync();

            var resultDto = new BlogPostDto
            {
                PostId = post.PostId,
                UserId = post.UserId,
                Title = post.Title,
                Content = post.Content,
                Category = post.Category,
                CreatedAt = post.CreatedAt,
                UpdatedAt = post.UpdatedAt,
                ImgPath = post.ImgPath,
                UserName = user.UserName
            };

            return CreatedAtAction(nameof(GetBlogPost), new { id = post.PostId },
                new { Message = "Blog post created successfully.", Data = resultDto });
        }

        // PUT: api/BlogPosts/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePost(int id, [FromForm] UpdateBlogPostDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new
                {
                    Message = "Invalid data.",
                    Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });

            var post = await _context.BlogPosts.FindAsync(id);
            if (post == null)
                return NotFound(new { Message = $"Blog post with id = {id} not found." });

            var updatedFields = new List<string>();

            if (!string.IsNullOrEmpty(dto.Title) && dto.Title != post.Title)
            {
                post.Title = dto.Title;
                updatedFields.Add("Title");
            }

            if (!string.IsNullOrEmpty(dto.Content) && dto.Content != post.Content)
            {
                post.Content = dto.Content;
                updatedFields.Add("Content");
            }

            if (dto.Category != null && dto.Category != post.Category)
            {
                post.Category = dto.Category;
                updatedFields.Add("Category");
            }

            if (dto.Img != null && dto.Img.Length > 0)
            {
                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(dto.Img.FileName);
                var filePath = Path.Combine(_uploadsFolder, uniqueFileName);

                try
                {
                    using var fileStream = new FileStream(filePath, FileMode.Create);
                    await dto.Img.CopyToAsync(fileStream);
                    post.ImgPath = $"/assets/Upload_Image/{uniqueFileName}";
                    updatedFields.Add("ImgPath");
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { Message = "Error saving image file.", Detail = ex.Message });
                }
            }

            if (updatedFields.Count > 0)
            {
                post.UpdatedAt = DateTime.UtcNow;
                _context.Entry(post).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                Message = "Blog post updated successfully.",
                UpdatedFields = updatedFields.Count > 0 ? updatedFields : new List<string> { "No fields were updated." }
            });
        }

        // DELETE: api/BlogPosts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBlogPost(int id)
        {
            var blogPost = await _context.BlogPosts.FindAsync(id);
            if (blogPost == null)
                return NotFound(new { Message = $"Blog post with id = {id} not found." });

            _context.BlogPosts.Remove(blogPost);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Blog post deleted successfully." });
        }
    }
}