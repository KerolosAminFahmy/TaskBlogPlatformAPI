using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using System.Net;
using System.Security.Claims;
using Task_Blog_Platform.Data;
using Task_Blog_Platform.Model;
using Task_Blog_Platform.Model.DTO;

namespace Task_Blog_Platform.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController
        (ApplicationDbContext _db , 
        UserManager<ApplicationUser> _userManager,
        SignInManager<ApplicationUser> _signInManager) : ControllerBase
    {
        private APIResponse _APIResponse=new APIResponse();
        [HttpPost(nameof(Login))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Login(LoginRequestDTO userDTO)
        {
            
                var user = _db.Users
                .FirstOrDefault(u => u.UserName.ToLower() == userDTO.UserName.ToLower());

                bool isValid = await _userManager.CheckPasswordAsync(user, userDTO.Password);


                if (user == null || !isValid )
                {
                    _APIResponse.ErrorMessages.Add("Invalid User Name Or Password");
                    _APIResponse.IsSuccess = false;
                    _APIResponse.StatusCode= HttpStatusCode.BadRequest;
                    return BadRequest(_APIResponse);
                }
                await _signInManager.SignInAsync(user, true);
                _APIResponse.IsSuccess = true;
                _APIResponse.StatusCode = HttpStatusCode.OK;
                return Ok(_APIResponse);

        }
        [HttpPost(nameof(Register))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Register(RegisterRequestDTO registerRequestDTO)
        {
            var isUnique = _db.Users.SingleOrDefault(m=>m.Email.ToLower() == registerRequestDTO.Email.ToLower() ||
            m.UserName.ToLower()== registerRequestDTO.UserName.ToLower());
            if(isUnique != null)
            {
                _APIResponse.ErrorMessages.Add("User Name Or Email Are Used Before");
                _APIResponse.IsSuccess = false;
                _APIResponse.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_APIResponse);
            }

            ApplicationUser user = new()
            {
                UserName = registerRequestDTO.UserName,
                Email = registerRequestDTO.Email,
                NormalizedEmail = registerRequestDTO.Email.ToUpper(),
               
            };

            try
            {
                var result = await _userManager.CreateAsync(user, registerRequestDTO.Password);
                if (result.Succeeded)
                {
                    
                    var userToReturn = _db.Users.Select(m=>new UserDto()
                    {
                        UserName=m.UserName,
                        Email=m.Email
                    })
                        .FirstOrDefault(u => u.UserName == registerRequestDTO.UserName);

                  
                    _APIResponse.IsSuccess = true;
                    _APIResponse.StatusCode = HttpStatusCode.OK;
                    _APIResponse.Result = userToReturn;
                    return Ok(_APIResponse);


                }
                else
                {
                    _APIResponse.ErrorMessages.Add("Something is wrong Try Again Later");
                    _APIResponse.IsSuccess = false;
                    _APIResponse.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_APIResponse);

                }
            }
            catch (Exception e)
            {
                _APIResponse.ErrorMessages.Add(e.Message);
                _APIResponse.IsSuccess = false;
                _APIResponse.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_APIResponse);
            }

        }
        [HttpPost(nameof(Logout))]
        public async Task<IActionResult> Logout() {
            await _signInManager.SignOutAsync();   
            return Ok();
        }


        [HttpPost(nameof(FollowUser))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult FollowUser(string Id)
        {
            if (string.IsNullOrEmpty(User.FindFirstValue(ClaimTypes.NameIdentifier)))
            {
                _APIResponse.ErrorMessages.Add("You should login first please");
                _APIResponse.IsSuccess = false;
                _APIResponse.StatusCode = HttpStatusCode.Unauthorized;
                return Unauthorized(_APIResponse);
            }
            var user=_db.Users.SingleOrDefault(m=>m.Id == Id);
            if(user == null)
            {
                _APIResponse.ErrorMessages.Add("User Not Found");
                _APIResponse.IsSuccess = false;
                _APIResponse.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_APIResponse);
            }
            if(Id == User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                _APIResponse.ErrorMessages.Add("You Cannot Follow YourSelf");
                _APIResponse.IsSuccess = false;
                _APIResponse.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_APIResponse);
            }
            var follow = new Follow()
            {
                FollowerId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                FollowingId = Id
            };
            _db.Follows.Add(follow);
            _db.SaveChanges();  
        
            _APIResponse.IsSuccess = true;
            _APIResponse.StatusCode = HttpStatusCode.OK;
            _APIResponse.Result = follow;
            return Ok(_APIResponse);
        }
        [HttpPost(nameof(ViewAccount))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ViewAccount()
        {
            if (string.IsNullOrEmpty(User.FindFirstValue(ClaimTypes.NameIdentifier)))
            {
                _APIResponse.ErrorMessages.Add("You should login first please");
                _APIResponse.IsSuccess = false;
                _APIResponse.StatusCode = HttpStatusCode.Unauthorized;
                return Unauthorized(_APIResponse);
            }
            var user = _db.Users.SingleOrDefault(m => m.Id == User.FindFirstValue(ClaimTypes.NameIdentifier));

            var Account = new UserAccountDTO()
            {
                OwnInfo = new UserDto() { UserName = user.UserName, Email = user.Email },
                Followers= await 
                _db.Follows.Include(n=>n.Follower)
                .Where(m=>m.FollowingId== user.Id)
                .Select(m=>new UserDto {UserName=m.Follower.UserName,Email=m.Follower.Email }).ToListAsync(),
                Following = await _db.Follows.Include(n => n.Following).
                Where(m => m.FollowerId == user.Id).Select(m => new UserDto { UserName = m.Following.UserName, Email = m.Following.Email }).ToListAsync(),
                blogs = await _db.Blogs.Where(m=>m.AuthorId== user.Id).ToListAsync(),
            };
           

            _APIResponse.IsSuccess = true;
            _APIResponse.StatusCode = HttpStatusCode.OK;
            _APIResponse.Result = Account;
            return Ok(_APIResponse);
        }
    }
}
