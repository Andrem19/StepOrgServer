using Microsoft.AspNetCore.Identity;

namespace StepOrg.Entities
{
    public class User : IdentityUser<int>
    {
        public string InviteCode { get; set; }
        public List<UserGroups>? UserGroups { get; set; } = new();
    }
}
