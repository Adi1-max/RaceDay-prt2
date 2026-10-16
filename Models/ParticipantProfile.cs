namespace RaceDay222.Models
{
    public class ParticipantProfile
    {

    public int ParticipantProfileID { get; set; }
        public int ParticipantID { get; set; }
        public string? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhone { get; set; }
    }
}
