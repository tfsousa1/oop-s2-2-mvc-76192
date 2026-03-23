using Microsoft.AspNetCore.Mvc;

namespace FoodSafetyInspectionTracker.Controllers
{
    public class ErrorDemoController : Controller
    {
        public IActionResult Index()
        {
            throw new Exception("Demo exception for testing global error handling.");
        }
    }
}