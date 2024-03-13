using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace TicketingSystem.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult GetJwtToken()
        {
            // 在這個示例中，我們假設通過某種方式驗證了用戶的身份，並獲取了用戶的一些信息（例如用戶名）。
            // 這裡只是一個示例，你可以根據你的實際需求進行修改。
            var username = "example_user";

            // 生成 JWT 令牌
            var token = GenerateJwtToken(username);



            // 將令牌返回給客戶端
            return Ok(new { token });
        }
        private string GenerateJwtToken(string username)
        {
            // 生成 JWT 令牌的一些設置，包括加密密鑰、發行者、接收者、有效期等。
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("your_secret_key_here");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                new Claim(ClaimTypes.Name, username)
                }),
                Expires = DateTime.UtcNow.AddHours(1), // 令牌的有效期為 1 小時
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            // 生成 JWT 令牌
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

    }
}
