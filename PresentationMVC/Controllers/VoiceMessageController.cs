namespace PresentationMVC.Controllers
{
    [Authorize]  // ← بدل [Authorize(Policy = "JwtPolicy")]
    [ApiController]
    [Route("api/[controller]")]
    public class VoiceMessageController : AppController
    {
        private readonly IWebHostEnvironment _env;
        private const long MaxFileSizeBytes = 10 * 1024 * 1024;

        public VoiceMessageController(IWebHostEnvironment env)
            => _env = env;

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile audio)
        {
            if (audio is null || audio.Length == 0)
                return BadRequest(new { error = "No audio file received." });

            if (audio.Length > MaxFileSizeBytes)
                return BadRequest(new { error = "File exceeds 10 MB limit." });

            var allowedPrefixes = new[] { "audio/webm", "audio/ogg", "audio/mp4", "audio/mpeg" };
            var contentType = audio.ContentType?.ToLower() ?? "";
            if (!allowedPrefixes.Any(p => contentType.StartsWith(p)))
                return BadRequest(new { error = "Unsupported audio format." });

            // ✅ FIX: تحقق إن الـ UserId موجود فعلاً
            if (string.IsNullOrEmpty(UserId))
                return Unauthorized(new { error = "User identity not found." });

            var folder = Path.Combine(_env.WebRootPath, "voice");
            Directory.CreateDirectory(folder);

            var ext = Path.GetExtension(audio.FileName).ToLower();
            if (string.IsNullOrEmpty(ext)) ext = ".webm";

            var fileName = $"{UserId}_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}{ext}";
            var filePath = Path.Combine(folder, fileName);

            // ✅ FIX: Path traversal guard
            var resolvedPath = Path.GetFullPath(filePath);
            var resolvedFolder = Path.GetFullPath(folder);
            if (!resolvedPath.StartsWith(resolvedFolder))
                return BadRequest(new { error = "Invalid file path." });

            await using var stream = System.IO.File.Create(filePath);
            await audio.CopyToAsync(stream);

            var audioUrl = $"/voice/{fileName}";
            return Ok(new { audioUrl });
        }
    }
}