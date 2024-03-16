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
    public class PaidsController : ControllerBase
    {
        private readonly ActService actService;
        private readonly PaidsService paidsService;
        private readonly JwtService jwtService;
        private readonly TicketsUUIDService ticketsUUIDService;
        private readonly UsersService usersService;
        private readonly EmailHelper emailHelper;

        public PaidsController(PaidsService paidsService , JwtService jwtService
            , TicketsUUIDService ticketsUUIDService
            , UsersService usersService
            , EmailHelper emailHelper)
        {
            this.actService = actService;
            this.paidsService = paidsService;
            this.jwtService = jwtService;
            this.ticketsUUIDService = ticketsUUIDService;
            this.usersService = usersService;
            this.emailHelper = emailHelper;
        }
        /// <summary>
        /// 取得購買紀錄
        /// </summary>
        /// <returns></returns>
        [HttpGet("getPaidsList")]
        public IEnumerable<PaidsInfo> GetPaidsList()
        {
            string token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            var userId = jwtService.GetUserIdFromToken(token);
            return paidsService.GetPaidsList(userId);
        }
        /// <summary>
        /// resend Email
        /// </summary>
        /// <returns></returns>
        [HttpPost("reSendEmail")]
        public IActionResult ReSendEmail([FromBody]ReSendEmailPaidId reSendEmailPaidId)
        {
            try
            {
                int Paids_Id = reSendEmailPaidId.id;
                string token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                var userId = jwtService.GetUserIdFromToken(token);
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

                return Ok(new { success = true});
            }
            catch (Exception ex)
            {
                Log.Error("resend", ex);
                return BadRequest(ex.Message);
            }
        }
    }
    public class ReSendEmailPaidId
    {
        public int id { get; set; }
    }

}
