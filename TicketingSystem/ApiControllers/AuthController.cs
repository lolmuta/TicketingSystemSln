using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TicketingSystem.LoginUtil;
using TicketingSystem.Models;
using TicketingSystem.Repo;

namespace TicketingSystem.ApiControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly JwtService jwtService;
        private readonly UsersService usersService;

        public AuthController(JwtService jwtService, UsersService usersService
           )
        {
            this.jwtService = jwtService;
            this.usersService = usersService;
        }
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginModel model)
        {
            Log.Information("call login");
            // 從 model 中獲取用戶名和密碼
            string userId = model.userId;
            string userPwd = model.userPwd;
            bool isValid = usersService.ValidUser(userId, userPwd);
            if (!isValid)
            {
                return BadRequest("驗證錯誤");
            }
            var token = jwtService.GenerateJwtToken(userId);
            return Ok(new { token });
        }
        [HttpGet("checkLogin")]
        public IActionResult CheckLogin()
        {
            try
            {
                string token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                var userId = jwtService.GetUserIdFromToken(token);
                if(userId == null)
                {
                    return Ok(new { success = false, message = "未登入" });
                }

                return Ok(new { success = true, message = "登入" });
            }
            catch (Exception ex)
            {
                return BadRequest("GetUserInfo 錯誤" + ex.Message);
            }
        }
        [HttpGet("getUserInfo")]
        public IActionResult GetUserInfo()
        {
            try
            {
                string token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                var userId = jwtService.GetUserIdFromToken(token);
                UserInfo userInfo = usersService.GetUserInfo(userId);
                return Ok(new { success = true, message = userInfo });
            }
            catch (Exception ex)
            {
                return BadRequest("GetUserInfo 錯誤" + ex.Message);
            }
        }
        [HttpPost("createAccount")]
        public IActionResult CreateAccount([FromBody] CreateUserModel model)
        {
            string userId = model.userId;
            string userName = model.userName;
            string pwd = model.userPwd;
            string email = model.userEmail;
            string error = usersService.CreateUser(userId, userName, pwd, email);
            if (!string.IsNullOrWhiteSpace(error))
            {
                return Ok(new { success = false, message = error });
            }

            return Ok(new { success = true });
        }
    }

    public class CreateUserModel
    {

        public string userName { get; set; }
        public string userPwd { get; set; }
        public string userId { get; set; }
        public string userEmail { get; set; }
    }

    public class LoginModel
    {
        public string userId { get;  set; }
        public string userPwd { get; set; }
    }
}
