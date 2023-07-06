using Microsoft.AspNetCore.Mvc;

namespace TinyLink.Api.Controllers.Base
{
    public abstract class AuthenticatedControllerBase : ControllerBase
    {

        protected string GetSubjectId()
        {
            if (HttpContext.User.Identity != null)
            {
                if (HttpContext.User.Identity.IsAuthenticated)
                {
                    var subject = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "sub")?.Value;
                    if (string.IsNullOrWhiteSpace(subject))
                    {
                        if (HttpContext.User.Claims.Any(c =>
                                c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"))
                        {
                            subject = HttpContext.User.Claims.FirstOrDefault(c =>
                                c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
                        }
                    }
                    return subject;
                }
            }

            return string.Empty;
        }

    }
}
