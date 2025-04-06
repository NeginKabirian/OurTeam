using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Planner.Helpers
{
    public class AdminAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var role = context.HttpContext.Request.Cookies["user_role"];

            if (string.IsNullOrEmpty(role) || role.ToLower() != "admin")
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
            }
        }
    }
}
