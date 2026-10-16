namespace RaceDay222MVC.Models
{
    public class ResultsViewModel
    {
        public int ResultID { get; set; }


    public int EventID { get; set; }

        public int ParticipantID { get; set; }

        public string EventName { get; set; } = string.Empty;

        public string ParticipantName { get; set; } = string.Empty;

        public int Position { get; set; }

        public TimeSpan? Time { get; set; }
    }


}
