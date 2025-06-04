using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using SWD_BLDONATION.Utils;

namespace SWD_BLDONATION.DTOs
{
    public class UpdateBlogPostDto
    {
        [MaxLength(255, ErrorMessage = "Title cannot exceed 255 characters.")]
        public string? Title { get; set; }

        public string? Content { get; set; }

        [MaxLength(100, ErrorMessage = "Category cannot exceed 100 characters.")]
        public string? Category { get; set; }

        [Utils.FileExtensions(Extensions = "jpg,jpeg,png", ErrorMessage = "Only .jpg, .jpeg, and .png files are allowed.")]
        [MaxFileSize(5 * 1024 * 1024, ErrorMessage = "File size cannot exceed 5MB.")]
        public IFormFile? Img { get; set; }
    }
}