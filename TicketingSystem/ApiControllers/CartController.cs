using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TicketingSystem.LoginUtil;
using TicketingSystem.Models;
using TicketingSystem.Repo;
using TicketingSystem.Service;

namespace TicketingSystem.ApiControllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly CartService cartService;
        private readonly JwtService jwtService;

        public CartController(CartService cartService, JwtService jwtService)
        {
            this.cartService = cartService;
            this.jwtService = jwtService;
        }
        [HttpPost("postAddToTempCart")]
        public IActionResult PostAddToTempCart([FromBody] TempCartModel model)
        {
            string token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            var userId = jwtService.GetUserIdFromToken(token);
            string result = cartService.PostAddToTempCart(model.id, model.count, userId);
            if (string.IsNullOrWhiteSpace(result))
            {
                return Ok(new { success = true });
            }
            else
            {
                return Ok(new { success = false, message = result });
            }
        }
        [HttpGet("getCartList")]
        public IEnumerable<CartInfo> GetCartList()
        {
            string token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            var userId = jwtService.GetUserIdFromToken(token);
            return cartService.GetCartList(userId);
        }
        [HttpGet("getCurBuyCount")]
        public int GetCurBuyCount(int id)
        {
            string token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            var userId = jwtService.GetUserIdFromToken(token);
            return cartService.GetCurBuyCount(id, userId);
        }
        [HttpPost("postDeleteCart")]
        public IActionResult PostDeleteCart([FromBody] DeleteCart deleteCart)
        {
            string token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            var userId = jwtService.GetUserIdFromToken(token);
            string result = cartService.PostDeleteCart(deleteCart.id, userId);
            if (string.IsNullOrWhiteSpace(result))
            {
                return Ok(new { success = true });
            }
            else
            {
                return Ok(new { success = false, message = result });
            }
        }
        [HttpPost("postPaid")]
        public IActionResult PostPaid()
        {
            string token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            var userId = jwtService.GetUserIdFromToken(token);
            string result = cartService.PostPaid(userId);
            if (string.IsNullOrWhiteSpace(result))
            {
                return Ok(new { success = true });
            }
            else
            {
                return Ok(new { success = false, message = result });
            }
        }
    }

    public class DeleteCart
    {
        public int id { get; set; }
    }

    public class TempCartModel
    {
        public int id { get; set; }
        public int count { get; set; }
    }
}
