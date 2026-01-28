using Microsoft.AspNetCore.Mvc;
using RideHailing.Web.Data;
using RideHailing.Web.Models;

namespace RideHailing.Web.Controllers;

public class TripController : Controller
{

    // GET: /Trip
    public IActionResult Index()
    {
        var trips = FakeDatabase.GetAllTrips();
        return View(trips);
    }

    // GET: /Trip/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: /Trip/Create
    [HttpPost]
    public IActionResult Create(Trip trip)
    {
        trip.Id = DateTime.Now.Ticks;
        trip.CreatedAt = DateTime.Now;
        trip.Status = TripStatus.Accepted;

        trip.Driver = new Driver
        {
            Name = "Nguyễn Văn A",
            Phone = "0909123456",
            VehicleNumber = "59X1-123.45",
            Region = trip.Region
        };

        FakeDatabase.AddTrip(trip);

        return RedirectToAction("Result", new { id = trip.Id });
    }


    // GET: /Trip/Result/1
    public IActionResult Result(long id, Region region)
    {
        var trip = FakeDatabase
            .GetTripsByRegion(region)
            .FirstOrDefault(t => t.Id == id);

        if (trip == null) return NotFound();
        return View(trip);
    }

    // GET: /Trip/Details/1
    public IActionResult Details(long id)
    {
        var trip = FakeDatabase.GetAllTrips()
            .FirstOrDefault(t => t.Id == id);

        if (trip == null) return NotFound();

        return View(trip);
    }
}
