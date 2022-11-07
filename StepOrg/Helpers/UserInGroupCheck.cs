using Microsoft.EntityFrameworkCore;
using StepOrg.Data;
using StepOrg.Entities;

namespace StepOrg.Helpers
{
    public static class UserInGroupCheck
    {
        public static async Task<bool> UserInGroup(ApplicationDbContext _data, string groupId, User user)
        {
            var group = await _data.Groups.Include(c => c.UsersInGroup).FirstOrDefaultAsync(x => x.Id == Convert.ToInt64(groupId));
            if (group != null)
            {
                int index = group.UsersInGroup.FindIndex(x => x.UserId == user.Id.ToString());
                if (index != -1)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
