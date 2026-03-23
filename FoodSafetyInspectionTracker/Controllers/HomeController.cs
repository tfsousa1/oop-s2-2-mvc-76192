using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using FoodSafetyInspectionTracker.Models;

namespace FoodSafetyInspectionTracker.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        _logger.LogInformation("Home page visited");
        return View();
    }

    public IActionResult Privacy()
    {
        _logger.LogInformation("Privacy page visited");
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        _logger.LogError("An error occurred and the Error view was displayed");
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}