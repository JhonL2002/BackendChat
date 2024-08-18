using System.ComponentModel.DataAnnotations;

namespace BackendChat.DTOs
{
    public class UpdateUserDto
    {
        [Required]
        [RegularExpression("^[A-Za-z]+$", ErrorMessage = "First Name must contain letters")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "First Name must be at least 3 characters")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [RegularExpression("^[A-Za-z]+$", ErrorMessage = "Last Name must contain letters")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Last Name must be at least 3 characters")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^[a-zA-Z][a-zA-Z0-9_.]{2,19}$", ErrorMessage = "Nickname must contain letters")]
        [StringLength(20, ErrorMessage = "Nickname must be at least 3 characters")]
        public string Nickname { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime DOB { get; set; }

        public IFormFile? ProfilePicture { get; set; }
        public string? ProfilePictureUrl { get; set; }

        public bool EmailConfirmed { get; set; } = false;

        public string? EmailConfirmationToken { get; set; } = string.Empty;

    }
}
