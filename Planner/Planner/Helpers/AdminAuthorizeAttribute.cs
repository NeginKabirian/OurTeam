
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Planner.Helpers
{
    public class AdminAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var role = context.HttpContext.Request.Cookies["user_role"];
            Console.WriteLine(context.HttpContext.Request.Cookies["user_role"]);
     
            if (string.IsNullOrEmpty(role) || role != "Admin")
            {
                context.Result = new ForbidResult();
            }
            
        }
    }
    public class UserAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var role = context.HttpContext.Request.Cookies["user_role"];

            if (string.IsNullOrEmpty(role) || role != "OrdinaryUser")
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
