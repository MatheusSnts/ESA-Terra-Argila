using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ESA_Terra_Argila.Services;
using System.Security.Claims;

namespace ESA_Terra_Argila.Controllers
{
    [Authorize]
    public class UserActivityController : Controller
    {
        private readonly IUserActivityService _activityService;

        public UserActivityController(IUserActivityService activityService)
        {
            _activityService = activityService;
        }

        public async Task<IActionResult> Index(
            string? activityType = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var activities = await _activityService.GetUserActivitiesAsync(
                userId,
                activityType,
                startDate,
                endDate
            );

            return View(activities);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UserActivities(
            string userId,
            string? activityType = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("ID do usuário não fornecido");
            }

            var isAuthorized = await _activityService.IsAuthorizedToViewActivitiesAsync(User, userId);
            if (!isAuthorized)
            {
                return Forbid();
            }

            var activities = await _activityService.GetUserActivitiesAsync(
                userId,
                activityType,
                startDate,
                endDate
            );
            return View("Index", activities);
        }
    }
}
