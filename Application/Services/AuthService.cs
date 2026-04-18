using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BoxFusion.Application.DTOs;
using BoxFusion.API.BoxFusion.Domain.Entities;
using BoxFusion.API.DTOs;

namespace BoxFusion.API.Application.Services;

public class AuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _config;

    public AuthService(UserManager<ApplicationUser> userManager, IConfiguration config)
    {
        _userManager = userManager;
        _config = config;
    }

    public async Task<AuthResult> RegisterAsync(RegisterDto dto)
    {
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
            return new AuthResult { Success = false, Message = "User already exists" };

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return new AuthResult { Success = false, Message = string.Join(", ", result.Errors.Select(e => e.Description)) };

        // როლის მინიჭება
        if (!string.IsNullOrEmpty(dto.Role))
        {
            await _userManager.AddToRoleAsync(user, dto.Role);
        }

        return new AuthResult { Success = true, Message = "User registered successfully" };
    }

    public async Task<string?> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null) return null;

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!isPasswordValid) return null;

        // მომხმარებლის როლების წამოღება
        var roles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email ?? "")
        };
        
        // როლების ჩამატება Claim-ებში
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        // 1. JWT Key-ს დაზღვევა (თუ კონფიგურაციაში არ არის, იყენებს სათადარიგოს)
        var jwtKey = _config["Jwt:Key"] ?? "BoxFusion_Super_Secret_Key_2026_Secure_String_32_Chars!!";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // 2. ვადის (ExpireMinutes) დაზღვევა
        var expireStr = _config["Jwt:ExpireMinutes"] ?? "60";
        if (!double.TryParse(expireStr, out double expireMinutes))
        {
            expireMinutes = 60; // Default მნიშვნელობა შეცდომის შემთხვევაში
        }

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"] ?? "BoxFusion",
            audience: _config["Jwt:Audience"] ?? "BoxFusionUsers",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expireMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}