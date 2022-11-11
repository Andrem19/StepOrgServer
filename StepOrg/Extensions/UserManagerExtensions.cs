using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StepOrg.Entities;
using System.Security.Claims;

namespace StepOrg.Extensions
{
    public static class UserManagerExtensions
    {
        public static async Task<User> FindByEmailFromClaimsPrinciple(this UserManager<User> input, ClaimsPrincipal user)
        {
            var email = user.FindFirstValue(ClaimTypes.Email);

            return await input.Users.SingleOrDefaultAsync(x => x.Email == email);
        }
        public static async Task<User> FindByEmailWithGroupsAsync(this UserManager<User> input, ClaimsPrincipal user)
        {
            var email = user.FindFirstValue(ClaimTypes.Email);

            return await input.Users.Include(x => x.UserGroups).SingleOrDefaultAsync(x => x.Email == email);
        }
    }
}
