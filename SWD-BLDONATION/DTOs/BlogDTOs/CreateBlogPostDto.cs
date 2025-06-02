using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace SWD_BLDONATION.DTOs
{
    public class CreateBlogPostDto
    {
        [Required]
        [DefaultValue(0)] // For int, default 0
        public int UserId { get; set; }

        [Required]
        [MaxLength(255)]
        [DefaultValue("")]
        public string Title { get; set; } = null!;

        [DefaultValue("")]
        public string? Content { get; set; }

        [MaxLength(100)]
        [DefaultValue("")]
        public string? Category { get; set; }

        public IFormFile? Img { get; set; }  // File upload
    }
}
