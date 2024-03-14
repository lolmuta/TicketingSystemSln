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
        [HttpGet("getActList")]
        public IEnumerable<ActInfo> GetProductList()
        {
            return actService.GetProductList();
        }
        [HttpGet("getActDetail")]
        public ActDetail GetActDetail(int id)
        {
            return actService.GetActDetail(id);
        }
        [HttpGet("getDDlActDates")]
        public IEnumerable<SelectListItem> GetDDlActDates(int id)
        {
            return actService.GetDDlActDates(id);
        }
        [HttpGet("getTicketCount")]
        public int GetTicketCount(int id)
        {
            return actService.GetTicketCount(id);
        }

    }

}
