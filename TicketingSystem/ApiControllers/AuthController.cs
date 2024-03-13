using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TicketingSystem.LoginUtil;

namespace TicketingSystem.ApiControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly JwtService jwtService;

        public AuthController(JwtService jwtService)
        {
            this.jwtService = jwtService;
        }
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginModel model)
        {
            // 從 model 中獲取用戶名和密碼
            string userId = model.userId;
            string userPwd = model.userPwd;

            var token = jwtService.GenerateJwtToken(userId);
            return Ok(new { token });
        }
        [HttpGet("getUsernameFromToken")]
        public IActionResult GetUsernameFromToken()
        {
            string token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            var userId = jwtService.GetUserIdFromToken(token);
            return Ok(new { userId });
        }

    }

    public class LoginModel
    {
        public string userId { get; internal set; }
        public string userPwd { get; internal set; }
    }
}
