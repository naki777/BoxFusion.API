using Microsoft.AspNetCore.Mvc;
using BoxFusion.API.Application.Services;
using BoxFusion.Application.DTOs;

namespace BoxFusion.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var result = await _authService.RegisterAsync(dto);
        if (!result.Success)
            return BadRequest(result.Message);

        return Ok(result.Message);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var token = await _authService.LoginAsync(dto);
        if (token == null)
            return Unauthorized("არასწორი Email ან Password");

        return Ok(new { token });
    }
}