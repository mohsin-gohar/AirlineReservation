using AirlineReservation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AirlineReservation.Areas.Admin;

/// <summary>Session-based gate: only signed-in users with the Admin role reach admin actions.</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AdminAuthorizeAttribute : Attribute, IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        var session = context.HttpContext.Session;
        var userId = session.GetInt32("UserId");
        var role = session.GetInt32("UserRole");
        if (userId is null)
            context.Result = new RedirectToActionResult("Login", "Account", new { area = (string?)null });
        else if (role != (int)UserRole.Admin)
            context.Result = new RedirectToActionResult("Index", "Home", new { area = (string?)null });
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}