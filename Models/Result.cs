namespace RaceDay222.Models
{
    public class Result
    {
    public int ResultID { get; set; }
        public int EnrollmentID { get; set; }
        public int RaceID { get; set; }
        public TimeSpan FinishTime { get; set; }
        public TimeSpan ChipTime { get; set; }
        public int? Position { get; set; }
        public int? CategoryPosition { get; set; }
        public string status { get; set; } = string.Empty;
    }
}
