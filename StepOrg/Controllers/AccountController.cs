using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StepOrg.Data;
using StepOrg.DTOs;
using StepOrg.Entities;
using StepOrg.Errors;
using StepOrg.Extensions;
using StepOrg.Services;

namespace StepOrg.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly UserManager<User> _userManager;
        private readonly TokenService _tokenService;
        private readonly ImageService _imageService;
        private readonly IConfiguration _config;
        private readonly EmailService _emailService;
        private readonly ApplicationDbContext _context;
        public AccountController(ApplicationDbContext context, EmailService emailService, UserManager<User> userManager, TokenService tokenService, IConfiguration config, ImageService imageService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _config = config;
            _emailService = emailService;
            _imageService = imageService;
            _context = context;
        }
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            var user = await _userManager.FindByEmailFromClaimsPrinciple(HttpContext.User);

            return new UserDto
            {
                Email = user.Email,
                Token = await _tokenService.GenerateToken(user),
                DisplayName = user.UserName,
                InviteCode = user.InviteCode
            };
        }

        [HttpGet("emailexists")]
        public async Task<ActionResult<bool>> CheckEmailExistsAsync([FromQuery] string email)
        {
            return await _userManager.FindByEmailAsync(email) != null;
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);

            if (user == null || !await _userManager.CheckPasswordAsync(user, loginDto.Password))
                return Unauthorized();

            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                return BadRequest(new ProblemDetails { Title = "Please confirm your email" });
            }

            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Token = await _tokenService.GenerateToken(user),
                DisplayName = user.UserName,
                InviteCode = user.InviteCode
            };
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register(RegisterDto registerDto)
        {
            if (CheckEmailExistsAsync(registerDto.Email).Result.Value)
            {
                return new BadRequestObjectResult(new ApiValidationErrorResponse { Errors = new[] { "Email address is in use" } });
            }

            var user = new User
            {
                UserName = registerDto.DisplayName,
                Email = registerDto.Email,
                InviteCode = Guid.NewGuid().ToString().ToUpper().Substring(24)
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);
            await _userManager.AddToRoleAsync(user, "Member");

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }
                return ValidationProblem();
            }

            User userReturn = await _userManager.FindByEmailAsync(registerDto.Email);
            if (userReturn != null)
            {
                var token = _userManager.GenerateEmailConfirmationTokenAsync(userReturn);
                var confirmationLink = Url.Action("ConfirmEmail", "Account",
                    new { userId = userReturn.Id, token = token.Result }, Request.Scheme);
                await _emailService.SendEmail(user.Email, confirmationLink);
            }

            return Ok();
        }
        [HttpGet("ConfirmEmail")]
        public async Task<ActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null) return BadRequest();

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null) return BadRequest();

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded) return BadRequest();

            return RedirectPermanent("https://www.sushione.co.uk/account/email_success");
        }
        [Authorize]
        [HttpPost("Avatar")]
        public async Task<ActionResult> DownloadAvatar([FromForm] AvatarDto avatar, [FromQuery] string groupId)
        {
            if (avatar.File != null)
            {
                var user = await _userManager.FindByEmailFromClaimsPrinciple(HttpContext.User);
                var group = await _context.Groups.Include(c => c.UsersInGroup).FirstOrDefaultAsync(x => x.Id == Convert.ToInt64(groupId));
                if (group != null)
                {
                    var userInGroup = group.UsersInGroup.FirstOrDefault(x => x.UserId == user.Id.ToString());
                    if (userInGroup != null)
                    {
                        var imageResult = await _imageService.AddImageAsync(avatar.File);
                        if (userInGroup.AvatarPublicId != null)
                        {
                            await _imageService.DeleteImageAsync(userInGroup.AvatarPublicId);
                        }
                        userInGroup.AvatarUrl = imageResult.SecureUrl.ToString();
                        userInGroup.AvatarPublicId = imageResult.PublicId;
                        var result = await _context.SaveChangesAsync() > 0;

                        if (result) return Ok(userInGroup.AvatarUrl);

                        return BadRequest("Problem updating the user");
                    }
                }
            }
            return Ok();
        }
        [Authorize]
        [HttpGet("Avatar")]
        public async Task<ActionResult> GetAvatar([FromQuery] string groupId)
        {
            var user = await _userManager.FindByEmailFromClaimsPrinciple(HttpContext.User);
            var group = await _context.Groups.Include(c => c.UsersInGroup).FirstOrDefaultAsync(x => x.Id == Convert.ToInt64(groupId));
            if (group != null)
            {
                var userInGroup = group.UsersInGroup.FirstOrDefault(x => x.UserId == user.Id.ToString());
                if (!user.IsExistInGroup(group)) return BadRequest("User not exist in group");

                if (userInGroup.AvatarUrl != null)
                {
                    return Ok(userInGroup.AvatarUrl);
                }

            }
            return Ok();
        }
        [Authorize]
        [HttpGet("changeInviteCode")]
        public async Task<ActionResult<string>> ChangeInviteCode()
        {
            var user = await _userManager.FindByEmailFromClaimsPrinciple(HttpContext.User);
            string firstPartOfCode = Guid.NewGuid().ToString().ToUpper().Substring(24);
            string secondPartOfCode = Guid.NewGuid().ToString().ToUpper().Substring(24);
            user.InviteCode = firstPartOfCode + secondPartOfCode;
            await _userManager.UpdateAsync(user);
            return Ok(user.InviteCode);
        }
    }
}
