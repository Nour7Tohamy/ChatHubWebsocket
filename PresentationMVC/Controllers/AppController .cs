namespace PresentationMVC.Controllers
{
    public abstract class AppController : Controller
    {
        protected string UserId =>
            User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        protected string DisplayName =>
            User.FindFirstValue("displayName")
            ?? User.FindFirstValue(ClaimTypes.Name)
            ?? "Unknown";
    }
}