using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StepOrg.DTOs;
using StepOrg.Entities.ModulesStruct.Ads;
using StepOrg.Entities;
using StepOrg.Data;
using StepOrg.Extensions;
using Microsoft.EntityFrameworkCore;

namespace StepOrg.Controllers
{
    public class AdController : BaseApiController
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;
        public AdController(ApplicationDbContext context, UserManager<User> userManager, IMapper mapper)
        {
            _context = context;
            _userManager = userManager;
            _mapper = mapper;
        }
        [Authorize]
        [HttpPost]
        public async Task<ActionResult> CreateAd([FromBody] AdDto AdDto)
        {
            var user = await _userManager.FindByEmailFromClaimsPrinciple(HttpContext.User);
            var group = _context.Groups.Include(x => x.UsersInGroup).Include(o => o.Ads).FirstOrDefault(p => p.Id == AdDto.GroupId);
            var UserInGroup = group.UsersInGroup.FirstOrDefault(x => x.UserId == user.Id.ToString());
            Ad newAd = new();
            if (UserInGroup.Role == ROLE.CREATOR || UserInGroup.Role == ROLE.MODERATOR)
            {
                newAd.TextBody = AdDto.TextBody;
                newAd.AuthorName = user.NormalizedUserName;
                newAd.IsVoting = false;
                newAd.AuthorId = user.Id.ToString();
                if (UserInGroup.Role == ROLE.CREATOR)
                {
                    newAd.IsVoting = AdDto.IsVoting;
                    if (newAd.IsVoting == true)
                    {
                        var Voting = new Voting();
                        Voting.IsSecret = AdDto.Voting.IsSecret;
                        for (int i = 0; i < AdDto.Voting.Variants.Count; i++)
                        {
                            Variant variant = new();
                            variant.TextBody = AdDto.Voting.Variants[i].TextBody;
                            variant.Percent = 0;
                            Voting.Variants.Add(variant);
                        }
                        newAd.Voting = Voting;
                    }
                }
                group.Ads.Add(newAd);
                await _context.SaveChangesAsync();
            }
            return Ok();
        }
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<List<AdDto>>> GetAllAds([FromQuery] int GroupId)
        {
            var user = await _userManager.FindByEmailFromClaimsPrinciple(HttpContext.User);
            var group = _context.Groups.Include(o => o.UsersInGroup).Include(x => x.Ads).ThenInclude(c => c.Voting).ThenInclude(d => d.Variants).FirstOrDefault(p => p.Id == GroupId);
            if (group == null) return BadRequest();
            var userInGroup = group.UsersInGroup.FindIndex(x => x.UserId == user.Id.ToString());
            if (userInGroup == -1) return BadRequest();
            List<AdDto> ads = _mapper.Map<List<AdDto>>(group.Ads);
            for (int i = 0; i < ads.Count; i++)
            {
                var authorId = ads[i].AuthorId;
                var authorUserInGroup = group.UsersInGroup.FirstOrDefault(x => x.UserId == authorId);
                if (authorUserInGroup?.AvatarUrl == null)
                {
                    ads[i].AuthorAvatar = "https://res.cloudinary.com/dodijnztn/image/upload/v1661087474/HomeOrganizer/touch-face_l81h24.png";
                }
                else
                {
                    ads[i].AuthorAvatar = authorUserInGroup.AvatarUrl;
                }
            }
            return Ok(ads);
        }
        [Authorize]
        [HttpPut]
        public async Task<ActionResult<AdDto>> Vote([FromBody] MyVoteDto Vote)
        {
            var user = await _userManager.FindByEmailFromClaimsPrinciple(HttpContext.User);
            var group = _context.Groups.Include(x => x.Ads).Include(l => l.UsersInGroup).FirstOrDefault(p => p.Id == Vote.GroupId);
            if (group == null) return BadRequest();
            var userInGroup = group.UsersInGroup.FirstOrDefault(x => x.UserId == user.Id.ToString());
            if (userInGroup == null) return BadRequest();
            var Ad = group.Ads.FirstOrDefault(x => x.Id == Vote.AdId);
            if (Ad == null) return BadRequest();
            if (Ad.Voting.IsVote.Contains(user.Id.ToString()))
            {
                return BadRequest(new ProblemDetails { Title = "You are voted allready" });
            }
            var variant = Ad.Voting.Variants.FirstOrDefault(x => x.Id == Vote.VariantId);
            if (variant == null) return BadRequest();
            if (Ad.Voting.IsSecret == false)
            {
                variant.Names.Add(user.UserName);
            }
            variant.Percent += userInGroup.Percent;
            Ad.Voting.IsVote.Add(user.Id.ToString());
            await _context.SaveChangesAsync();
            return Ok(_mapper.Map<AdDto>(Ad));
        }
    }
}
