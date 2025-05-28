using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace SWD_BLDONATION.DTOs
{
    public class CreateBlogPostDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        [MaxLength(255)]
        public string Title { get; set; } = null!;

        public string? Content { get; set; }

        [MaxLength(100)]
        public string? Category { get; set; }

        public IFormFile? Img { get; set; }  // File upload
    }
}
