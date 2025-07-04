namespace SWD_BLDONATION.DTOs.NotificationDTOs
{
    public class NotificationDto
    {
        public int NotificationId { get; set; }
        public int? UserId { get; set; }
        public string? Message { get; set; }
        public string? Type { get; set; }
        public string? Status { get; set; }
        public DateTime? SentAt { get; set; }
    }
}
