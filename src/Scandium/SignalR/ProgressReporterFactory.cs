using Microsoft.AspNetCore.SignalR;

namespace Scandium.SignalR;

public class ProgressReporterFactory
{
    private readonly IHubContext<LoadingBarHub> _progressHubContext;

    public ProgressReporterFactory(IHubContext<LoadingBarHub> progressHubContext)
    {
        _progressHubContext = progressHubContext;
    }

    public IProgress<double> GetLoadingBarReporter(string connectionId)
    {
        if (connectionId == null) return new Progress<double>();

        double fractionComplete = 0;
        IProgress<double> progress = new Progress<double>(fractionDone =>
        {
            fractionComplete += fractionDone;
            _progressHubContext.Clients.Client(connectionId).SendAsync("updateLoadingBar", fractionComplete);
        });

        return progress;
    }
}