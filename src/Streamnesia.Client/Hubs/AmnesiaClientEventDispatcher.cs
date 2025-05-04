using Microsoft.AspNetCore.SignalR;
using Streamnesia.Core;

namespace Streamnesia.Client.Hubs;

public class AmnesiaClientEventDispatcher
{
    private readonly IHubContext<StatusHub> _hubContext;
    private readonly ILogger<AmnesiaClientEventDispatcher> _logger;

    public AmnesiaClientEventDispatcher(
        IAmnesiaClient amnesiaClient,
        IHubContext<StatusHub> hubContext,
        ILogger<AmnesiaClientEventDispatcher> logger)
    {
        _hubContext = hubContext;
        _logger = logger;

        amnesiaClient.StateChangedAsync += OnAmnesiaClientStateChangedAsync;

        // TODO(spelos): Extract into its own dispatcher maybe?
        AppDomain.CurrentDomain.UnhandledException += async (sender, args) =>
        {
            var exception = (Exception)args.ExceptionObject;
            await ReportUnhandledExceptionAsync(exception);
        };

        TaskScheduler.UnobservedTaskException += async (sender, args) =>
        {
            await ReportUnhandledExceptionAsync(args.Exception);
            args.SetObserved(); // Prevents crashing the app
        };
    }

    private async Task OnAmnesiaClientStateChangedAsync(object? sender, AmnesiaClientState state, string message)
    {
        try
        {
            var uiState = state.ToUiState();
            _logger.LogInformation("Broadcasting AmnesiaClient state: {State}", uiState.ToString());
            await _hubContext.Clients.All.SendAsync("AmnesiaStateChanged", uiState.ToString(), message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to broadcast AmnesiaClient state change.");
        }
    }

    private async Task ReportUnhandledExceptionAsync(Exception ex)
    {
        try
        {
            await _hubContext.Clients.All.SendCoreAsync(
                "GlobalException",
                [new {
                    Message = ex.Message,
                    StackTrace = ex.StackTrace,
                    Time = DateTime.UtcNow
                }]
            );
        }
        catch
        {
            _logger.LogError("The exception handler failed");
        }
    }
}
