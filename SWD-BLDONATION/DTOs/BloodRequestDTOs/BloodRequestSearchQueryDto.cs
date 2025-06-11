namespace SWD_BLDONATION.DTOs.BloodRequestDTOs
{
    public class BloodRequestSearchQueryDto
    {
        public int? Id { get; set; }
        public int? UserId { get; set; }
        public int? BloodTypeId { get; set; }
        public int? BloodComponentId { get; set; }
        public string? Status { get; set; }
        public string? Location { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}