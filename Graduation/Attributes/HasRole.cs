using Graduation.Enums;
using Microsoft.AspNetCore.Authorization;

namespace Graduation.Attributes
{
    public class HasRole: AuthorizeAttribute
    {
        public HasRole(Roles role)
        {
            Roles = role.ToString();
        }
    }
}
