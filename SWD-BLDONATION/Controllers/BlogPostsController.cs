using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SWD_BLDONATION.DTOs;
using SWD_BLDONATION.Models.Generated;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SWD_BLDONATION.Controllers
{
    public class CreateBlogPostFormDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        public string Category { get; set; }

        public IFormFile Img { get; set; }
    }

    public class UpdateBlogPostFormDto
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string Category { get; set; }
        public IFormFile Img { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class BlogPostsController : ControllerBase
    {
        private readonly BloodDonationContext _context;

        public BlogPostsController(BloodDonationContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BlogPostDto>>> GetBlogPosts()
        {
            var posts = await _context.BlogPosts
                .Select(post => new BlogPostDto
                {
                    PostId = post.PostId,
                    UserId = post.UserId,
                    Title = post.Title,
                    Content = post.Content,
                    Category = post.Category,
                    CreatedAt = post.CreatedAt,
                    UpdatedAt = post.UpdatedAt,
                    ImgPath = post.ImgPath
                })
                .ToListAsync();

            return Ok(new { Message = "Lấy danh sách bài viết thành công.", Data = posts });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BlogPostDto>> GetBlogPost(int id)
        {
            var post = await _context.BlogPosts
                .Where(p => p.PostId == id)
                .Select(post => new BlogPostDto
                {
                    PostId = post.PostId,
                    UserId = post.UserId,
                    Title = post.Title,
                    Content = post.Content,
                    Category = post.Category,
                    CreatedAt = post.CreatedAt,
                    UpdatedAt = post.UpdatedAt,
                    ImgPath = post.ImgPath
                })
                .FirstOrDefaultAsync();

            if (post == null)
                return NotFound(new { Message = $"Không tìm thấy bài viết với id = {id}." });

            return Ok(new { Message = "Lấy bài viết thành công.", Data = post });
        }

        [HttpPost]
        public async Task<ActionResult<BlogPostDto>> CreatePost([FromForm] CreateBlogPostFormDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { Message = "Dữ liệu không hợp lệ.", Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });

            var post = new BlogPost
            {
                UserId = dto.UserId,
                Title = dto.Title,
                Content = dto.Content,
                Category = dto.Category,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            if (dto.Img != null && dto.Img.Length > 0)
            {
                var uploadsFolder = Path.Combine("C:\\Users\\RAZER\\Downloads\\SWD\\SWD392_Blood_Donation_Project\\src\\assets\\Upload_Image");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(dto.Img.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                try
                {
                    using var fileStream = new FileStream(filePath, FileMode.Create);
                    await dto.Img.CopyToAsync(fileStream);
                    post.ImgPath = "/assets/Upload_Image/" + uniqueFileName;
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { Message = "Lỗi khi lưu file ảnh.", Detail = ex.Message });
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
                ImgPath = post.ImgPath
            };

            return CreatedAtAction(nameof(GetBlogPost), new { id = post.PostId }, new { Message = "Tạo bài viết thành công.", Data = resultDto });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePost(int id, [FromForm] UpdateBlogPostFormDto dto)
        {
            var post = await _context.BlogPosts.FindAsync(id);
            if (post == null)
                return NotFound(new { Message = $"Không tìm thấy bài viết với id = {id}." });

            if (dto.Title != null)
                post.Title = dto.Title;
            if (dto.Content != null)
                post.Content = dto.Content;
            if (dto.Category != null)
                post.Category = dto.Category;

            if (dto.Img != null && dto.Img.Length > 0)
            {
                var uploadsFolder = Path.Combine("C:\\Users\\RAZER\\Downloads\\SWD\\SWD392_Blood_Donation_Project\\src\\assets\\Upload_Image");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(dto.Img.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                try
                {
                    using var fileStream = new FileStream(filePath, FileMode.Create);
                    await dto.Img.CopyToAsync(fileStream);
                    post.ImgPath = "/assets/Upload_Image/" + uniqueFileName;
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { Message = "Lỗi khi lưu file ảnh.", Detail = ex.Message });
                }
            }

            post.UpdatedAt = DateTime.Now;
            _context.Entry(post).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Cập nhật bài viết thành công." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBlogPost(int id)
        {
            var blogPost = await _context.BlogPosts.FindAsync(id);
            if (blogPost == null)
                return NotFound(new { Message = $"Không tìm thấy bài viết với id = {id}." });

            _context.BlogPosts.Remove(blogPost);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Xóa bài viết thành công." });
        }
    }
}
