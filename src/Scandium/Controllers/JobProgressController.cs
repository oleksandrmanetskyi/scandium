using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Scandium.Services;

namespace Scandium.Controllers;

public class JobProgressController: Controller
{
    private readonly ILogger<JobProgressController> _logger;
    private readonly JobService _jobService;

    public JobProgressController(ILogger<JobProgressController> logger, JobService jobService)
    {
        _logger = logger;
        _jobService = jobService;
    }

    [HttpPost]
    [Route("progress-report/subscribe")]
    public async Task<IActionResult> SubscribeConnectionId([FromForm] string jobId, [FromForm] string connectionId)
    {
        _logger.LogInformation("Subscribing connection id: {connectionId} to {jobId}", connectionId, jobId);
        await _jobService.SubscribeConnectionId(new ObjectId(jobId), connectionId);
        return new OkResult();
    }
}