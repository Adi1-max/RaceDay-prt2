
namespace RaceData222.DTOs
{
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}