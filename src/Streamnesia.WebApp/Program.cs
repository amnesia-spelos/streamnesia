using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Streamnesia.CommandProcessing;
using Streamnesia.Payloads;
using Streamnesia.Twitch;
using Streamnesia.WebApp.Hubs;
using Streamnesia.WebApp;
using Newtonsoft.Json;
using System.IO;
using Streamnesia.Core;

const string StreamnesiaConfigFile = "streamnesia-config.json";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSignalR();

builder.Services.AddSingleton(p => GetStreamnesiaConfig());

builder.Services.AddSingleton<UpdateHub>();
builder.Services.AddSingleton<StreamnesiaHub>();
builder.Services.AddSingleton<Random>();
builder.Services.AddSingleton<ICommandPoll, CommandPoll>();
builder.Services.AddSingleton<CommandQueue>();
builder.Services.AddSingleton<IPayloadLoader, LocalPayloadLoader>();
builder.Services.AddSingleton<TwitchBot>();
builder.Services.AddSingleton<PollState>();

builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapHub<UpdateHub>("/updatehub");

app.Run();

StreamnesiaConfig GetStreamnesiaConfig()
{
    if (!File.Exists(StreamnesiaConfigFile))
    {
        var config = new StreamnesiaConfig();
        File.WriteAllText(StreamnesiaConfigFile, JsonConvert.SerializeObject(config, Formatting.Indented));
        return config;
    }

    return JsonConvert.DeserializeObject<StreamnesiaConfig>(File.ReadAllText(StreamnesiaConfigFile));
}