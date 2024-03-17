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
    public class ActController : ControllerBase
    {
        private readonly ActService actService;
        private readonly JwtService jwtService;

        public ActController(ActService actService, JwtService jwtService)
        {
            this.actService = actService;
            this.jwtService = jwtService;
        }
        /// <summary>
        /// 取得所有活動列表
        /// </summary>
        /// <returns>活動列表</returns>
        [HttpGet("getActList")]
        public IEnumerable<ActInfo> GetActList()
        {
            return actService.GetActList();
        }
        /// <summary>
        /// 取得活動明細
        /// </summary>
        /// <param name="id">活動代號</param>
        /// <returns>活動明細</returns>
        [HttpGet("getActDetail")]
        public ActDetail GetActDetail(int id)
        {
            return actService.GetActDetail(id);
        }
        /// <summary>
        /// 取得指定活動的時刻的下拉
        /// </summary>
        /// <param name="id">活動代號</param>
        /// <returns></returns>
        [HttpGet("getDDlActDates")]
        public IEnumerable<SelectListItem> GetDDlActDates(int id)
        {
            return actService.GetDDlActDates(id);
        }
        /// <summary>
        /// 取得指定活動的庫存票數
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("getTicketCount")]
        public int GetTicketCount(int id)
        {
            return actService.GetTicketCount(id);
        }
        /// <summary>
        /// 取得場次票的單價
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("getTicketPrice")]
        public decimal GetTicketPrice(int id)
        {
            return actService.GetTicketPrice(id);
        }
    }

}
