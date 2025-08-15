using Application.Interface;
using Application.Models.DTOs;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IUserService userService, ILogger<AuthController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login([FromBody] LoginUserDto loginDto)
    {
        try
        {
            var result = await _userService.AuthenticateAsync(loginDto);
            return Ok(ApiResponse<AuthResponseDto>.SuccessResult(result, "Giriş başarılı"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<AuthResponseDto>.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login error");
            return StatusCode(500, ApiResponse<AuthResponseDto>.ErrorResult("Bir hata oluştu"));
        }
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Register([FromBody] RegisterUserDto registerDto)
    {
        try
        {
            var result = await _userService.RegisterAndAuthenticateAsync(registerDto);
            return Ok(ApiResponse<AuthResponseDto>.SuccessResult(result, "Kayıt ve giriş başarılı"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<AuthResponseDto>.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Register error");
            return StatusCode(500, ApiResponse<AuthResponseDto>.ErrorResult("Bir hata oluştu"));
        }
    }

    [HttpPost("oauth")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> OAuth([FromBody] RegisterWithProviderDto providerDto)
    {
        try
        {
            var result = await _userService.AuthenticateWithProviderAsync(providerDto);
            return Ok(ApiResponse<AuthResponseDto>.SuccessResult(result, "OAuth giriş başarılı"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<AuthResponseDto>.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OAuth error");
            return StatusCode(500, ApiResponse<AuthResponseDto>.ErrorResult("Bir hata oluştu"));
        }
    }

    [HttpPost("anonymous")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> AuthenticateAnonymous([FromBody] AnonymousUserRequestDto requestDto)
    {
        try
        {
            var result = await _userService.AuthenticateAnonymousAsync(requestDto);
            return Ok(ApiResponse<AuthResponseDto>.SuccessResult(result, "Anonim giriş başarılı"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AuthenticateAnonymous error");
            return StatusCode(500, ApiResponse<AuthResponseDto>.ErrorResult("Bir hata oluştu"));
        }
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<UserResponseDto>>> GetCurrentUser()
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<UserResponseDto>.ErrorResult("Geçersiz token"));
            }

            var user = await _userService.GetByIdAsync(userId);
            if (user == null)
            {
                return NotFound(ApiResponse<UserResponseDto>.ErrorResult("Kullanıcı bulunamadı"));
            }

            return Ok(ApiResponse<UserResponseDto>.SuccessResult(user));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetCurrentUser error");
            return StatusCode(500, ApiResponse<UserResponseDto>.ErrorResult("Bir hata oluştu"));
        }
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        try
        {
            // Bu implementasyon refresh token'ları veritabanında saklamayı gerektirir
            // Şimdilik basit bir implementation
            return BadRequest(ApiResponse<AuthResponseDto>.ErrorResult("Refresh token özelliği henüz aktif değil"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RefreshToken error");
            return StatusCode(500, ApiResponse<AuthResponseDto>.ErrorResult("Bir hata oluştu"));
        }
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult<ApiResponse>> Logout()
    {
        try
        {
            // JWT'lerde logout genellikle client-side yapılır (token'ı local storage'dan sil)
            // Server-side blacklist gerekiyorsa, token'ı bir blacklist cache'ine eklenebilir
            return Ok(ApiResponse.SuccessResult("Çıkış başarılı"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Logout error");
            return StatusCode(500, ApiResponse.ErrorResult("Bir hata oluştu"));
        }
    }
}
