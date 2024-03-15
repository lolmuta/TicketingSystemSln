using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TicketingSystem.LoginUtil;
using TicketingSystem.Models;
using TicketingSystem.Repo;
using TicketingSystem.Service;
using TicketingSystem.Utils;

namespace TicketingSystem.ApiControllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly CartService cartService;
        private readonly JwtService jwtService;
        private readonly TicketsUUIDService ticketsUUIDService;
        private readonly EmailHelper emailHelper;
        private readonly UsersService usersService;
        private readonly PaidsService paidsService;

        public CartController(CartService cartService
            , JwtService jwtService
            , TicketsUUIDService ticketsUUIDService
            , EmailHelper emailHelper
            , UsersService usersService
            , PaidsService paidsService)
        {
            this.cartService = cartService;
            this.jwtService = jwtService;
            this.ticketsUUIDService = ticketsUUIDService;
            this.emailHelper = emailHelper;
            this.usersService = usersService;
            this.paidsService = paidsService;
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
            try
            {
                string token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                var userId = jwtService.GetUserIdFromToken(token);
                string result = cartService.PostPaid(userId, out int Paids_Id);
                if (!string.IsNullOrWhiteSpace(result))
                {
                    return Ok(new { success = false, message = result });
                }
                var ticketsInfo = ticketsUUIDService.GetTicketsInfoByPaidId(Paids_Id);
                var userInfo = usersService.GetUserInfo(userId);
                MailInfo mailInfo = emailHelper.GetEmailBody(ticketsInfo, userInfo);

                string sendResult = emailHelper.Send(mailInfo);

                if (!string.IsNullOrWhiteSpace(sendResult))
                {
                    paidsService.UpdateSendMailStatusToTrue(Paids_Id);
                    return Ok(new { success = false, message = sendResult });
                }

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
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
