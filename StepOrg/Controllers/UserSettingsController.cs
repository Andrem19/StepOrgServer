using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StepOrg.Data;
using StepOrg.DTOs;
using StepOrg.Entities;
using StepOrg.Extensions;

namespace StepOrg.Controllers
{
    public class UserSettingsController : BaseApiController
    {

        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        public UserSettingsController(IMapper mapper, ApplicationDbContext context, UserManager<User> userManager)
        {
            _userManager = userManager;
            _context = context;
            _mapper = mapper;
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<List<UserInGroupDto>>> GetAllUsersInGroup(int groupId)
        {
            var user = await _userManager.FindByEmailFromClaimsPrinciple(HttpContext.User);
            var group = await _context.Groups.Include(x => x.UsersInGroup).FirstOrDefaultAsync(x => x.Id == groupId);
            if (group == null) return BadRequest();

            if (group.UsersInGroup.FindIndex(x => x.UserId == user.Id.ToString()) == -1)
            {
                return BadRequest();
            }
            return Ok(_mapper.Map<List<UserInGroupDto>>(group.UsersInGroup));
        }
        [Authorize]
        [HttpDelete]
        public async Task<ActionResult> DeleteUserFromGroup([FromQuery] string userId, [FromQuery] string groupId)
        {
            var user = await _userManager.FindByEmailFromClaimsPrinciple(HttpContext.User);
            var group = await _context.Groups.Include(x => x.UsersInGroup).FirstOrDefaultAsync(x => x.Id.ToString() == groupId);
            if (group == null) return BadRequest();

            var Creators = group.UsersInGroup.Where(x => x.Role == ROLE.CREATOR).ToList();
            var CreaterWhoRequest = Creators.FirstOrDefault(x => x.UserId == user.Id.ToString());
            if (CreaterWhoRequest == null)
                return BadRequest();

            var userInGroup = group.UsersInGroup.FirstOrDefault(x => x.UserId == userId);
            if (userInGroup != null)
            {
                group.UsersInGroup.Remove(userInGroup);
                await _context.SaveChangesAsync();
            }

            return Ok();
        }
        [Authorize]
        [HttpPut]
        public async Task<ActionResult<List<UserInGroupDto>>> SetPercentage([FromBody] List<UserInGroupDto> users, [FromQuery] int groupId)
        {
            var group = _context.Groups.Include(x => x.UsersInGroup).FirstOrDefault(x => x.Id == groupId);
            if (group == null) return BadRequest();

            var user = await _userManager.FindByEmailFromClaimsPrinciple(HttpContext.User);
            if (user == null) return BadRequest();
            if (group.UsersInGroup.Exists(x => x.UserId == user.Id.ToString() && x.Role == ROLE.CREATOR))
            {
                if (group.UsersInGroup.Count == users.Count)
                {
                    for (int i = 0; i < group.UsersInGroup.Count; i++)
                    {
                        for (int t = 0; t < users.Count; t++)
                        {
                            if (group.UsersInGroup[i].UserId == users[t].UserId)
                            {
                                group.UsersInGroup[i].Percent = users[t].Percent;
                            }
                        }
                    }
                    await _context.SaveChangesAsync();
                }
                else return BadRequest();
            }
            else return BadRequest();

            return Ok(_mapper.Map<List<UserInGroup>>(group.UsersInGroup));
        }
    }
}
