using System.Security.Claims;

namespace GP_Server.Application.Helpers;

public static class UserHelper
{
    public static string GetUserId(ClaimsPrincipal user)
    {
        var res = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (res != null)
        {
            return res;
        }
        throw new ArgumentNullException();
    }
    public static bool IsSuperAdmin(ClaimsPrincipal user)
    {
        return user.IsInRole("SuperAdmin");
    }
}
