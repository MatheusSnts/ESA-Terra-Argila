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
                
            // Verifica se o usuário está bloqueado
            if (user.LockoutEnabled && user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow)
            {
                // Usuário está bloqueado, não autoriza
                return;
            }

            // Verifica se o usuário é um customer (consumidor)
            var isCustomer = await _userManager.IsInRoleAsync(user, "Customer");
            
            // Se for um customer, aprova automaticamente sem verificar AcceptedByAdmin
            if (isCustomer)
            {
                context.Succeed(requirement);
                return;
            }

            // Para Vendor e Supplier, continua verificando se foram aprovados pelo admin
            var isVendorOrSupplier = await _userManager.IsInRoleAsync(user, "Vendor") || await _userManager.IsInRoleAsync(user, "Supplier");

            if (isVendorOrSupplier && user.AcceptedByAdmin)
            {
                context.Succeed(requirement);
            }
        }
    }
}
