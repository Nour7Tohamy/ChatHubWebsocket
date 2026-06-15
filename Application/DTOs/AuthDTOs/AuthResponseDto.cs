namespace Application.DTOs.AuthDTOs;

public class AuthResponseDto
{
    public string Token { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
}