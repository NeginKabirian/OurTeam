using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Planner.Models.DTO
{
    public class UserDTO
    {
        [Required]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
