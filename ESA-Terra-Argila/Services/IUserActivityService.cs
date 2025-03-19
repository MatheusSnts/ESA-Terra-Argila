using ESA_Terra_Argila.Models;
using System.Security.Claims;

namespace ESA_Terra_Argila.Services
{
    public interface IUserActivityService
    {
        Task<IEnumerable<UserActivity>> GetUserActivitiesAsync(
            string userId,
            string? activityType = null,
            DateTime? startDate = null,
            DateTime? endDate = null);

        Task<bool> IsAuthorizedToViewActivitiesAsync(ClaimsPrincipal user, string targetUserId);

        Task LogActivityAsync(
            string userId,
            string activityType,
            string description,
            bool isSuccess,
            string? additionalInfo = null
        );
    }
} 