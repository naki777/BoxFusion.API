namespace BoxFusion.Application.DTOs;

public class RegisterDto
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string Role { get; set; } = "Buyer"; // ან "Seller"
}