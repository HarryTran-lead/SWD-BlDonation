using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace SWD_BLDONATION.DTOs
{
    public class UpdateBlogPostDto
    {
        [MaxLength(255)]
        [DefaultValue("")]
        public string? Title { get; set; }

        [DefaultValue("")]
        public string? Content { get; set; }

        [MaxLength(100)]
        [DefaultValue("")]
        public string? Category { get; set; }

        public IFormFile? Img { get; set; }
    }
}
