using System.ComponentModel.DataAnnotations;

namespace Planner.Models
{
    public enum UserRole
    {
        OrdinaryUser = 0,
        Admin = 1
    }

    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public UserRole Role { get; set; }

        public ICollection<TaskItem> Tasks { get; set; }
        public ICollection<Session> Sessions { get; set; }
    }

}
