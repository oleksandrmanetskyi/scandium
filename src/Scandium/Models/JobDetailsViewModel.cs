using Scandium.Entities;

namespace Scandium.Models;

public class JobDetailsViewModel
{
    public string Id { get; set; } = string.Empty;
    public DateTime CreateDateTime { get; set; }
    public JobState State { get; set; }
    public string Result { get; set; } = string.Empty;
}