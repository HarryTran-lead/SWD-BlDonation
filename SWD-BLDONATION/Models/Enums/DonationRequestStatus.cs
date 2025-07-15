namespace SWD_BLDONATION.Models.Enums
{
    public enum DonationRequestStatus : byte
    {
        Pending = 0,      // 0 - pending
        Successful = 1,   // 1 - successful
        Cancelled = 2,     // 2 - cancelled
        Done = 3,     // 3 - Done in hospital,
        Stocked = 4, // 4 - Stocked in inventory
    }
}