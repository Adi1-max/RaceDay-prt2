
namespace RaceDayMVC.Models
{
    public class EventCategoryViewModel
    {
        public int EventCategoryID { get; set; }

        public int EventID { get; set; }

        public int CategoryID { get; set; }

        public string CategoryName { get; set; } = string.Empty;
    }
}