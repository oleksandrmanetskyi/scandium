using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Scandium.Entities;

namespace Scandium.Helpers;

public static class GetStateHelper
{
    public static HtmlString GetBootstrapState(this IHtmlHelper html, JobState state)
    {
        return state switch
        {
            JobState.Done => new HtmlString("bg-success"),
            JobState.Canceled => new HtmlString("bg-danger"),
            _ => new HtmlString("bg-primary")
        };
    }
}