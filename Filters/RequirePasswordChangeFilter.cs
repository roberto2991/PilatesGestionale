using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PilatesStudio.Filters;

/// <summary>
/// Reindirizza l'utente a ImpostaPassword se ha il claim "MustChangePassword".
/// Viene applicato globalmente in Program.cs.
/// </summary>
public class RequirePasswordChangeFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext ctx)
    {
        var user = ctx.HttpContext.User;

        if (user.Identity?.IsAuthenticated != true) return;
        if (!user.HasClaim("MustChangePassword", "true")) return;

        if (ctx.ActionDescriptor is not ControllerActionDescriptor action) return;

        // Permetti solo Account/ImpostaPassword e Account/Logout
        var controller = action.ControllerName;
        var actionName = action.ActionName;

        var isConsentita = controller == "Account" &&
            (actionName == "ImpostaPassword" || actionName == "Logout");

        if (!isConsentita)
        {
            ctx.Result = new RedirectToActionResult("ImpostaPassword", "Account", null);
        }
    }

    public void OnActionExecuted(ActionExecutedContext ctx) { }
}
