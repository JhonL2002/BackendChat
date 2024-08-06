using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendChat.Models
{
    public class AppUser
    {
        public int Id { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Nickname {  get; set; } = string.Empty;

        public DateTime DOB { get; set; }

        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string? ProfilePictureUrl { get; set; } = string.Empty;

        [NotMapped]
        public string FullName {  get; set; } = string.Empty;


    }
}
