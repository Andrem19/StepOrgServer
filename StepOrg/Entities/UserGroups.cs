namespace StepOrg.Entities
{
    public class UserGroups
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? PictureUrl { get; set; } = "";
        public int GroupId { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }
    }
}
