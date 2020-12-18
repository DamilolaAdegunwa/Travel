using Microsoft.AspNetCore.Mvc;

namespace Travel.WebAPI.Controllers
{
    public class HomeController : BaseController
    {

        [HttpGet]
        public IActionResult Index()
        {
            return Ok("Travel.Web Api is running");
        }
    }
}