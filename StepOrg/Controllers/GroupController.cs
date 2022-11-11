using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StepOrg.Data;
using StepOrg.DTOs;
using StepOrg.Entities;
using StepOrg.Extensions;
using StepOrg.Services;

namespace StepOrg.Controllers
{
    public class GroupController : BaseApiController
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;
        private readonly ImageService _imageService;
        public GroupController(ApplicationDbContext context, UserManager<User> userManager, IMapper mapper, ImageService imageService)
        {
            _context = context;
            _userManager = userManager;
            _mapper = mapper;
            _imageService = imageService;
        }
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<List<GroupDto>>> GetMyGroups()
        {
            var user = await _userManager.FindByEmailFromClaimsPrinciple(HttpContext.User);

            var groups = await _context.Groups.Include(c => c.UsersInGroup).Include(t => t.Ads).ToListAsync();

            var res = groups.Where(x => x.UsersInGroup.Exists(p => p.UserId == user.Id.ToString()));
            List<Group> groupToReturn = res.ToList();
            if (groupToReturn.Count < 1)
            {
                return Ok(null);
            }
            return Ok(_mapper.Map<List<GroupDto>>(groupToReturn));
        }

        [Authorize]
        [HttpGet("getbyid")]
        public async Task<ActionResult<GroupDto>> GetGroupById([FromQuery]int Id)
        {
            var user = await _userManager.FindByEmailFromClaimsPrinciple(HttpContext.User);

            var group = await _context.Groups.Include(c => c.UsersInGroup).Include(t => t.Ads).Include(u => u.Payloads).FirstOrDefaultAsync(x => x.Id == Id);
            var thisGroupInUser = user.UserGroups.FirstOrDefault(x => x.UserId == Id);
            if (group.PictureUrl != thisGroupInUser.PictureUrl)
            {
                thisGroupInUser.PictureUrl = group.PictureUrl;
                await _userManager.UpdateAsync(user);
            }
            return Ok(_mapper.Map<GroupDto>(group));
        }

        [Authorize]
        [HttpGet("payloads")]
        public async Task<ActionResult<List<GroupDto>>> GetMyGroupsWithPayload()
        {
            var user = await _userManager.FindByEmailFromClaimsPrinciple(HttpContext.User);

            var groups = await _context.Groups.Include(c => c.UsersInGroup).Include(t => t.Payloads).ThenInclude(a => a.Tasks).ToListAsync();

            var res = groups.Where(x => x.UsersInGroup.Exists(p => p.UserId == user.Id.ToString()));
            List<Group> groupToReturn = res.ToList();
            if (groupToReturn.Count < 1)
            {
                return Ok(null);
            }
            return Ok(_mapper.Map<List<GroupDto>>(groupToReturn));
        }
        [Authorize]
        [HttpGet("roleingroup")]
        public async Task<ActionResult<string>> GetMyRole([FromQuery] string groupId)
        {
            var user = await _userManager.FindByEmailFromClaimsPrinciple(HttpContext.User);

            var group = await _context.Groups.Include(c => c.UsersInGroup).FirstOrDefaultAsync(x => x.Id == Convert.ToInt64(groupId));

            var userInGroup = group.UsersInGroup.FirstOrDefault(x => x.UserId == user.Id.ToString());
            if (userInGroup != null)
            {
                return Ok(userInGroup.Role.ToString());
            }
            return Ok();
        }
        [Authorize]
        [HttpGet("create")]
        public async Task<ActionResult<GroupDto>> CreateGroup([FromQuery]string ShortName, [FromQuery]string Name)
        {
            Group group = new Group();
            group.ShortName = ShortName;
            group.GroupName = Name;
            var user = await _userManager.FindByEmailFromClaimsPrinciple(HttpContext.User);
            
            
            UserInGroup newuser = new();
            newuser.UserId = user.Id.ToString();
            newuser.Name = user.NormalizedUserName;
            newuser.Role = ROLE.CREATOR;
            group.UsersInGroup.Add(newuser);

            _context.Groups.Add(group);
            await _context.SaveChangesAsync();

            UserGroups userGroups = new();
            userGroups.GroupId = group.Id;
            userGroups.Name = group.ShortName;
            userGroups.PictureUrl = group.PictureUrl?? "";

            user.UserGroups.Add(userGroups);

            await _userManager.UpdateAsync(user);
            
            return Ok(_mapper.Map<GroupDto>(group));
        }
        [Authorize]
        [HttpGet("changeName")]
        public async Task<ActionResult> ChangeName([FromQuery] string ShortName, [FromQuery] string Name, [FromQuery] string groupId)
        {
            var user = await _userManager.FindByEmailFromClaimsPrinciple(HttpContext.User);
            var group = _context.Groups.Include(x => x.UsersInGroup).FirstOrDefault(x => x.Id == Convert.ToInt32(groupId));

            if (user.IsNotCreator(group))
                return BadRequest();

            group.ShortName = ShortName;
            group.GroupName = Name;
            await _context.SaveChangesAsync();

            var userGroup = user.UserGroups.FirstOrDefault(x => x.Id == group.Id);
            if (userGroup.Name != group.ShortName)
            {
                userGroup.Name = group.ShortName;
                await _userManager.UpdateAsync(user);
            }

            return Ok();
        }
        [Authorize]
        [HttpDelete]
        public async Task<ActionResult> DeleteGroup([FromQuery] int GroupId)
        {
            var group = _context.Groups.Include(x => x.UsersInGroup).FirstOrDefault(x => x.Id == GroupId);
            if (group == null) return BadRequest();

            var user = await _userManager.FindByEmailFromClaimsPrinciple(HttpContext.User);

            if (user.IsNotCreator(group))
                return BadRequest();
            
            if (!string.IsNullOrEmpty(group.PicturePublicId))
            {
                await _imageService.DeleteImageAsync(group.PicturePublicId);
            }
            _context.Groups.Remove(group);
            await _context.SaveChangesAsync();

            for (int i = 0; i < group.UsersInGroup.Count; i++)
            {
                var userFromGroup = await _userManager.FindByIdAsync(group.UsersInGroup[i].Id.ToString());
                int indexOfGroup = userFromGroup.UserGroups.FindIndex(x => x.Id == group.Id);
                userFromGroup.UserGroups.RemoveAt(indexOfGroup);
                await _userManager.UpdateAsync(userFromGroup);
            }
            return Ok();
        }
        [Authorize]
        [HttpPut("setRole")]
        public async Task<ActionResult> SetRole([FromBody] SetRoleDto SetRole)
        {
            var group = _context.Groups.Include(x => x.UsersInGroup).FirstOrDefault(x => x.Id == SetRole.GroupId);
            if (group == null) return BadRequest();

            var user = await _userManager.FindByEmailFromClaimsPrinciple(HttpContext.User);


            if (user.IsNotCreator(group) || user.LastCreatorWantToRemoveHisself(group, SetRole.UserId))
            {
                return BadRequest();
            }
            var userToChangeRole = group.UsersInGroup.FirstOrDefault(x => x.UserId.ToString() == SetRole.UserId);
            if (userToChangeRole != null)
                userToChangeRole.Role = SetRole.Role;
            await _context.SaveChangesAsync();

            return Ok();
        }

        [Authorize]
        [HttpPut("addUser")]
        public async Task<ActionResult<Group>> AddUser([FromQuery] string InviteCode, [FromQuery] int GroupId)
        {
            var group = _context.Groups.Include(x => x.UsersInGroup).FirstOrDefault(x => x.Id == GroupId);
            if (group == null)
                return BadRequest();
            var user = await _userManager.FindByEmailFromClaimsPrinciple(HttpContext.User);
            
            if (user.IsNotCreator(group))
                return BadRequest();

            var userToAdd = _userManager.Users.FirstOrDefault(x => x.InviteCode == InviteCode);
            UserInGroup newuser = new();
            newuser.UserId = userToAdd.Id.ToString();
            newuser.Name = userToAdd.UserName;
            newuser.Role = ROLE.MEMBER;
            newuser.Percent = 0;
            group.UsersInGroup.Add(newuser);

            await _context.SaveChangesAsync();

            UserGroups userGroup = new();
            userGroup.Id = group.Id;
            userGroup.Name = group.ShortName;
            userGroup.PictureUrl = group.PictureUrl;
            user.UserGroups.Add(userGroup);
            await _userManager.UpdateAsync(user);

            return group;
        }
        [Authorize]
        [HttpPost("Avatar")]
        public async Task<ActionResult> UploadGroupPicture([FromForm]AvatarDto avatar, [FromQuery] string groupId)
        {
            if (avatar.File != null)
            {
                var user = await _userManager.FindByEmailFromClaimsPrinciple(HttpContext.User);
                var group = await _context.Groups.Include(c => c.UsersInGroup).FirstOrDefaultAsync(x => x.Id == Convert.ToInt64(groupId));
                if (group == null)
                    return BadRequest();

                var userInGroup = group.UsersInGroup.FirstOrDefault(x => x.UserId == user.Id.ToString());
                if (user.IsNotCreator(group))
                    return BadRequest();
                var imageResult = await _imageService.AddImageAsync(avatar.File);
                if (group.PicturePublicId != null)
                {
                    await _imageService.DeleteImageAsync(group.PicturePublicId);
                }
                group.PictureUrl = imageResult.SecureUrl.ToString();
                group.PicturePublicId = imageResult.PublicId;
                var result = await _context.SaveChangesAsync() > 0;

                if (result) return Ok(userInGroup.AvatarUrl);

                return BadRequest("Problem updating the group");
            }
            return Ok();
        }

        [Authorize]
        [HttpGet("leaveGroup")]
        public async Task<ActionResult> LeaveGroup([FromQuery] int GroupId)
        {
            var group = _context.Groups.Include(x => x.UsersInGroup).FirstOrDefault(x => x.Id == GroupId);
            if (group == null)
                return BadRequest();
            var user = await _userManager.FindByEmailFromClaimsPrinciple(HttpContext.User);

            if (user.LastCreatorWantToLeaveGroup(group))
                return BadRequest("Last Creator Can't leave group");
            var userToRemove = group.UsersInGroup.FirstOrDefault(x => x.UserId == user.Id.ToString());
            if (userToRemove != null)
            {
                group.UsersInGroup.Remove(userToRemove);
                await _context.SaveChangesAsync();
            }
            return Ok();
        }
    }
}
