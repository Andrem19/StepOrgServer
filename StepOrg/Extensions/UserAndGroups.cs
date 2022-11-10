using StepOrg.DTOs;
using StepOrg.Entities;

namespace StepOrg.Extensions
{
    public static class UserAndGroups
    {
        public static bool IsNotCreator(this User user, Group group)
        {
            var Creators = group.UsersInGroup.Where(x => x.Role == ROLE.CREATOR).ToList();
            var CreaterWhoRequest = Creators.FirstOrDefault(x => x.UserId == user.Id.ToString());
            if (CreaterWhoRequest == null)
            {
                return true;
            }
            return false;
        }
        public static bool IsNotCreatorOrModerator(this User user, Group group)
        {
            var Creators = group.UsersInGroup.Where(x => x.Role == ROLE.CREATOR || x.Role == ROLE.MODERATOR).ToList();
            var CreaterWhoRequest = Creators.FirstOrDefault(x => x.UserId == user.Id.ToString());
            if (CreaterWhoRequest == null)
            {
                return true;
            }
            return false;
        }
        public static bool LastCreatorWantToRemoveHisself(this User user, Group group, string userId)
        {
            var Creators = group.UsersInGroup.Where(x => x.Role == ROLE.CREATOR).ToList();
            var CreaterWhoRequest = Creators.FirstOrDefault(x => x.UserId == user.Id.ToString());
            if (Creators.Count == 1 && userId == CreaterWhoRequest.UserId)
            {
                return true;
            }
            return false;
        }
        public static bool LastCreatorWantToLeaveGroup(this User user, Group group)
        {
            var Creators = group.UsersInGroup.Where(x => x.Role == ROLE.CREATOR).ToList();
            var CreaterWhoRequest = Creators.FirstOrDefault(x => x.UserId == user.Id.ToString());
            if (Creators.Exists(x => x.Id == user.Id))
            {
                if (Creators.Count == 1)
                {
                    return true;
                }
            }
            return false;
        }
        public static bool IsExistInGroup(this User user, Group group)
        {
            return group.UsersInGroup.Exists(x => x.Id == user.Id);
        }
    }
}
