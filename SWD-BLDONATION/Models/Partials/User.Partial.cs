using SWD_BLDONATION.Models.Enums;

namespace SWD_BLDONATION.Models.Generated
{
    public partial class User
    {
        public UserRole Role => (UserRole)(RoleBit ?? 0);

        public UserStatus Status => (UserStatus)(StatusBit ?? 0);

        public bool IsActive => Status == UserStatus.Active;
    }
}
