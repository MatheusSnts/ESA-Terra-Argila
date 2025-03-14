using ESA_Terra_Argila.Enums;

namespace ESA_Terra_Argila
{
    public static class UserRoleHelper
    {
        private static readonly Dictionary<UserRole, string> RoleTranslations = new()
        {
            { UserRole.Customer, "Consumidor" },
            { UserRole.Vendor, "Empresa" },
            { UserRole.Supplier, "Fornecedor" }
        };

        public static string GetRoleTranslation(this UserRole role)
        {
            return RoleTranslations.TryGetValue(role, out var translation) ? translation : role.ToString();
        }

        public static UserRole GetUserRoleFromString(string roleString)
        {
            if (Enum.TryParse(roleString, true, out UserRole role))
            {
                return role;
            }
            return default;
        }

    }
}
