
namespace RaceDay222MVC.Models
{
    public class EventViewModel
    {
        public int EventID { get; set; }

        public string EventName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime EventDate { get; set; }

        public string? Location { get; set; }
    }
}