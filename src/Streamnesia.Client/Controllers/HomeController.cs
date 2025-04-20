using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Streamnesia.Client.Models;
using Streamnesia.Core;

namespace Streamnesia.Client.Controllers;

public class HomeController(IConfigurationStorage cfgStorage) : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet("/settings")]
    public IActionResult Settings()
    {
        return View(new SettingsModel
        {
            AmnesiaClientConfig = cfgStorage.ReadAmnesiaClientConfig(),
            TwitchBotConfig = cfgStorage.ReadTwitchBotConfig(),
        });
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

    [HttpPost("/settings")]
    public IActionResult SaveSettings(SettingsModel model)
    {
        if (!ModelState.IsValid)
            return View("Settings", model);

        cfgStorage.WriteAmnesiaClientConfig(model.AmnesiaClientConfig);
        cfgStorage.WriteTwitchBotConfig(model.TwitchBotConfig);

        TempData["SuccessMessage"] = "Settings saved successfully!";
        return RedirectToAction("Settings");
    }
}
