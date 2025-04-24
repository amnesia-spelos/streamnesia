using Streamnesia.Client.Extensions;
using Streamnesia.Client.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();
builder.Services.AddStreamnesiaDependencies(builder.Configuration);
builder.Services.AddSingleton<AmnesiaClientEventDispatcher>();
builder.Services.AddSingleton<TwitchPollDispatcher>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapHub<StatusHub>("/statushub");
app.MapHub<OverlayHub>("/overlayhub");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// ensuring this gets activated on startup
app.Services.GetRequiredService<AmnesiaClientEventDispatcher>();
app.Services.GetRequiredService<TwitchPollDispatcher>();

await app.RunAsync();
