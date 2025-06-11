using System.ComponentModel.DataAnnotations;

namespace SWD_BLDONATION.Utils
{
    public class MaxFileSizeAttribute : ValidationAttribute
    {
        private readonly long _maxSize;

        public MaxFileSizeAttribute(long maxSize)
        {
            _maxSize = maxSize;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is IFormFile file && file.Length > _maxSize)
                return new ValidationResult(ErrorMessage);
            return ValidationResult.Success;
        }
    }
}
