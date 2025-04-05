namespace Planner.Models
{
    public class Session
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        public User User { get; set; }
        public string SessionToken { get; set; }
        public DateTime ExpirationTime { get; set; }
    }
}
