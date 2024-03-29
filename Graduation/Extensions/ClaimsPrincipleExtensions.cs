using System.Security.Claims;

namespace Graduation.Extensions
{
    public static class ClaimsPrincipleExtensions
    {
        public static int? Id(this ClaimsPrincipal claimsPrincipal)
        {
            var id = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);

            if (id == null)
                return null;

            return int.Parse(id);
        }
    }
}
