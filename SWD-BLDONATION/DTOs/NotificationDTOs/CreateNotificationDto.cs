namespace SWD_BLDONATION.DTOs.NotificationDTOs
{
    public class CreateNotificationDto
    {
        public int? UserId { get; set; }
        public string? Message { get; set; }
        public string? Type { get; set; }
    }
}
