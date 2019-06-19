using Microsoft.AspNetCore.Mvc;

namespace LiveScore.BetRadarSimulator.Controllers
{    
    public class HomeController : ControllerBase
    {
        public IActionResult Index()
            => new ContentResult
            {
                Content = "BetRadar Simulator API has been started."
            };
    }
}
