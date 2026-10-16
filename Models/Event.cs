namespace RaceDay222.Models
{
    public class Event
    {
        public int EventID { get; set; }
        public int OrganiserID { get; set; }
        public string EventName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string EventLocation { get; set; } = string.Empty;
        public  decimal Distance { get; set; }
        public string? Description { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public string status { get; set; } = string.Empty;
    }
}
