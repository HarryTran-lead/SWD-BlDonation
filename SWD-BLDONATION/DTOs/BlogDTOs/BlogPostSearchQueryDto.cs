namespace SWD_BLDONATION.DTOs
{
    public class BlogPostSearchQueryDto
    {
        public int? Id { get; set; }
        public string? Title { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}