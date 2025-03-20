<<<<<<< Updated upstream
using ESA_Terra_Argila.Models;
using System.Security.Claims;
=======
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ESA_Terra_Argila.Models;
>>>>>>> Stashed changes

namespace ESA_Terra_Argila.Services
{
    public interface IUserActivityService
    {
<<<<<<< Updated upstream
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
=======
        Task<IEnumerable<UserActivity>> GetUserActivitiesAsync(string userId, string activityType, DateTime? startDate, DateTime? endDate);
        Task<bool> IsAuthorizedToViewActivitiesAsync(System.Security.Claims.ClaimsPrincipal user, string userId);
>>>>>>> Stashed changes
    }
} 