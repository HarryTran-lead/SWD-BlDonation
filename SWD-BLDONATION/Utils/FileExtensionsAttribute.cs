using System.ComponentModel.DataAnnotations;

namespace SWD_BLDONATION.Utils
{
    public class FileExtensionsAttribute : ValidationAttribute
    {
        public string Extensions { get; set; }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is IFormFile file)
            {
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                var allowedExtensions = Extensions.Split(',').Select(e => $".{e.ToLowerInvariant()}").ToArray();
                if (!allowedExtensions.Contains(extension))
                    return new ValidationResult(ErrorMessage);
            }
            return ValidationResult.Success;
        }
    }
}
