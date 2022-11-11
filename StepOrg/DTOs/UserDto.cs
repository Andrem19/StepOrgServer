namespace StepOrg.DTOs
{
    public class UserDto
    {
        public int? Id { get; set; }
        public string Email { get; set; }
        public string DisplayName { get; set; }
        public string Token { get; set; }
        public string InviteCode { get; set; }
    }
}
