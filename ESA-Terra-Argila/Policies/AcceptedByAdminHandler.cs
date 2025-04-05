using ESA_Terra_Argila.Policies.AuthorizationRequirements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using ESA_Terra_Argila.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Net.Http;
namespace ESA_Terra_Argila.Policies
{

    public class AcceptedByAdminHandler : AuthorizationHandler<AcceptedByAdminRequirement>
    {
        private readonly UserManager<User> _userManager;

        public AcceptedByAdminHandler(UserManager<User> userManager)
        {
            _userManager = userManager;

        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AcceptedByAdminRequirement requirement)
        {
            var user = await _userManager.GetUserAsync(context.User);

            if (user == null)
                return;

            var isInRole = await _userManager.IsInRoleAsync(user, "Vendor") || await _userManager.IsInRoleAsync(user, "Supplier");

            if (isInRole && user.AcceptedByAdmin)
            {
                context.Succeed(requirement);
            }
        }
    }
}
