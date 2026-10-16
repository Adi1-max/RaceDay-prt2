namespace RaceDay222.Models
{
    public class Payment
    {
        public int PaymentID { get; set; }
        public int EnrollmentID { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string status { get; set; } = string.Empty;
    }
}
