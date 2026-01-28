using Microsoft.AspNetCore.Mvc;
using RideHailing.Web.Data;

namespace RideHailing.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        var trips = FakeDatabase.GetAllTrips();
        return View(trips);
    }

    public IActionResult Privacy()
    {
        return View();
    }
}
