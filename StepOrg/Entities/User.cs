using Microsoft.AspNetCore.Identity;

namespace StepOrg.Entities
{
    public class User : IdentityUser
    {
        public string InviteCode { get; set; }
    }
}
