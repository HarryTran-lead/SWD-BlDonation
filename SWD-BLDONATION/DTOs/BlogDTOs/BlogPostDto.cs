using System;

namespace SWD_BLDONATION.DTOs
{
    public class BlogPostDto
    {
        public int PostId { get; set; }
        public int? UserId { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? Category { get; set; }
        public string? ImgPath { get; set; }
        public string? UserName { get; set; }  // Thêm nếu muốn hiển thị tên user liên quan
    }
}
