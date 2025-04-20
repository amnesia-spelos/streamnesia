using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Streamnesia.Client.Models;

namespace Streamnesia.Client.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet("/settings")]
    public IActionResult Settings()
    {
        return View();
    }

    [HttpGet("/overlay")]
    public IActionResult Overlay()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
