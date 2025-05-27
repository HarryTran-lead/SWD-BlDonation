using System;
using System.ComponentModel.DataAnnotations;

namespace SWD_BLDONATION.DTOs
{
    public class UpdateBlogPostDto
    {
        [MaxLength(255)]
        public string? Title { get; set; }

        public string? Content { get; set; }

        [MaxLength(100)]
        public string? Category { get; set; }

        public IFormFile? Img { get; set; }
    }
}
