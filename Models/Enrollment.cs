namespace RaceDay222.Models
{
    public class Enrollment
    {
    public int EnrollmentID { get; set; }
        public int ParticipantID { get; set; }
        public int EventCategoryID { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public string status { get; set; } = string.Empty;
    }
}
