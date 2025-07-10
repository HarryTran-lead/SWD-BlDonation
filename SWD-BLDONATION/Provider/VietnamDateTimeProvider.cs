namespace SWD_BLDONATION.Provider
{
    public class VietnamDateTimeProvider
    {
        private static readonly TimeZoneInfo VietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

        public static DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, VietnamTimeZone);

        public static DateTime Today => Now.Date;

        public static DateTime AddMinutes(DateTime dateTime, double minutes)
        {
            return dateTime.AddMinutes(minutes);
        }
    }
}
