using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Bson;
using Scandium.Services;

namespace Scandium.SignalR;

public class JobProgressReporterFactory
{
    private readonly IHubContext<LoadingHub> _progressHubContext;
    private readonly JobService _jobService;

    public JobProgressReporterFactory(IHubContext<LoadingHub> progressHubContext, JobService jobService)
    {
        _progressHubContext = progressHubContext;
        _jobService = jobService;
    }

    public IProgress<double> GetLoadingBarReporter(ObjectId jobId)
    {
        double fractionComplete = 0;
        IProgress<double> progress = new Progress<double>(fractionDone =>
        {
            fractionComplete += fractionDone;
            var connectionIdList = _jobService.GetJobConnectionIdList(jobId).Result;
            foreach (var connectionId in connectionIdList)
            {
                _progressHubContext.Clients.Client(connectionId)
                    .SendAsync("updateLoadingBar", fractionComplete);
            }
        });

        return progress;
    }

    public StateReporter GetStateReporter(ObjectId jobId)
    {
        var stateReporter = new StateReporter
        {
            ReportCancelState = () =>
            {
                var connectionIdList = _jobService.GetJobConnectionIdList(jobId).Result;
                foreach (var connectionId in connectionIdList)
                {
                    _progressHubContext.Clients.Client(connectionId)
                        .SendAsync("canceledLoading");
                }
            },
            
            ReportDoneState = () =>
            {
                var connectionIdList = _jobService.GetJobConnectionIdList(jobId).Result;
                foreach (var connectionId in connectionIdList)
                {
                    _progressHubContext.Clients.Client(connectionId)
                        .SendAsync("doneLoading");
                }
            }
        };
        return stateReporter;
    }
}

public class StateReporter
{
    public Action ReportCancelState { get; set; }
    public Action ReportDoneState { get; set; }
}