namespace SWD_BLDONATION.DTOs.NotificationDTOs
{
    public class UpdateBulkNotificationStatusDto
    {
        public List<int>? NotificationIds { get; set; }
        public int? UserId { get; set; }
    }
}
