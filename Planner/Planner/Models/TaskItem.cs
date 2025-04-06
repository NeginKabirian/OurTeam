using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Planner.Models
{
    public class TaskItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }
       // public string Description { get; set; }
        public bool IsCompleted { get; set; }
		[Required]
		public DateTime Deadline { get; set; }
        [Required]
        public int UserId { get; set; }
        public User User { get; set; }
        //public string label;
        //public string state { get; set; }
    }

}
