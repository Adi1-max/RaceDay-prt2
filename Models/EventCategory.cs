namespace RaceDay222.Models
{
    public class EventCategory
    {
        public int EventCategoryID { get; set; }
        public int EventID { get; set; }
        public int CategoryID { get; set; }
        public decimal Price { get; set; }
        public int MaxParticipants { get; set; }
        public int? MinimumAge { get; set; }
        public int? MaximumAge { get; set; }
        public int? Gender { get; set; } // 0 for Male, 1 for Female, 2 for Other
    }
}
