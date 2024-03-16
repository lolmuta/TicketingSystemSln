using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Serilog;
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
        /// <summary>
        /// 將購票加入購物車
        /// </summary>
        /// <param name="model">購票資訊</param>
        /// <returns></returns>
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
        /// <summary>
        /// 取得購物車的商品列表
        /// </summary>
        /// <returns></returns>
        [HttpGet("getCartList")]
        public IEnumerable<CartInfo> GetCartList()
        {
            string token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            var userId = jwtService.GetUserIdFromToken(token);
            return cartService.GetCartList(userId);
        }
        /// <summary>
        /// 取得使用者購物庫內的指定活動的票數
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("getCurBuyCount")]
        public int GetCurBuyCount(int id)
        {
            string token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            var userId = jwtService.GetUserIdFromToken(token);
            return cartService.GetCurBuyCount(id, userId);
        }
        /// <summary>
        /// 刪除購物車內指定活的的所有票數
        /// </summary>
        /// <param name="deleteCart"></param>
        /// <returns></returns>
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
        /// <summary>
        /// 將購物車的的所有商品付款
        /// </summary>
        /// <returns></returns>
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

                Task task = Task.Run(() =>
                {
                    try
                    {
                        string sendResult = emailHelper.Send(mailInfo);

                        if (!string.IsNullOrWhiteSpace(sendResult))
                        {
                            Log.Error("寄信錯誤1", sendResult);
                        }
                        else
                        {
                            paidsService.UpdateSendMailStatusToTrue(Paids_Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error("寄信錯誤2", ex);
                    }
                });

                //string sendResult = emailHelper.Send(mailInfo);

                //if (!string.IsNullOrWhiteSpace(sendResult))
                //{
                //    paidsService.UpdateSendMailStatusToTrue(Paids_Id);
                //    return Ok(new { success = false, message = sendResult });
                //}

                return Ok(new { success = true, message = Paids_Id });
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
