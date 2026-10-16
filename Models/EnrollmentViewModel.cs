namespace RaceDay222MVC.Models
{
    public class EnrollmentViewModel
    {
        public int EnrollmentID { get; set; }

        public int EventID { get; set; }

        public int ParticipantID { get; set; }

        public string EventName { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;
    }
}