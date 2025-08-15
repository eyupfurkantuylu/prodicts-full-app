using Application.Interface;
using Application.Models.DTOs;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

/// <summary>
/// Authentication ve authorization işlemleri için endpoint'ler
/// </summary>
/// <remarks>
/// Bu controller aşağıdaki authentication yöntemlerini destekler:
/// 
/// **1. Traditional Authentication:**
/// - Email/Password ile kayıt
/// - Email/Password ile giriş
/// 
/// **2. OAuth Authentication:**
/// - Google OAuth
/// - Facebook OAuth  
/// - Apple OAuth
/// 
/// **3. Anonymous Authentication:**
/// - Device ID ile anonim token
/// - Kayıt olmadan uygulama kullanımı
/// 
/// **Token Bilgileri:**
/// - Registered users: 60 dakika geçerli
/// - Anonymous users: 24 saat geçerli
/// - Bearer token formatı: JWT
/// 
/// **Security:**
/// - Tüm token'lar HS256 algoritması ile imzalanır
/// - Development ortamında HTTP desteklenir
/// - Production'da HTTPS zorunludur
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Tags("Auth")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IUserService userService, ILogger<AuthController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Kullanıcı giriş işlemi
    /// </summary>
    /// <param name="loginDto">Email ve şifre bilgileri</param>
    /// <returns>JWT token ve kullanıcı bilgileri</returns>
    /// <response code="200">Giriş başarılı</response>
    /// <response code="401">Geçersiz email veya şifre</response>
    /// <response code="500">Sunucu hatası</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status500InternalServerError)]
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

    /// <summary>
    /// Yeni kullanıcı kaydı
    /// </summary>
    /// <param name="registerDto">Kullanıcı kayıt bilgileri</param>
    /// <returns>JWT token ve kullanıcı bilgileri</returns>
    /// <response code="200">Kayıt ve giriş başarılı</response>
    /// <response code="400">Email zaten kullanımda veya geçersiz veri</response>
    /// <response code="500">Sunucu hatası</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status500InternalServerError)]
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

    /// <summary>
    /// OAuth ile kullanıcı authentication (Google, Facebook, Apple)
    /// </summary>
    /// <param name="providerDto">OAuth provider bilgileri</param>
    /// <returns>JWT token ve kullanıcı bilgileri</returns>
    /// <response code="200">OAuth başarılı</response>
    /// <response code="400">Geçersiz provider bilgileri</response>
    /// <response code="500">Sunucu hatası</response>
    [HttpPost("oauth")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status500InternalServerError)]
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

    /// <summary>
    /// Anonim kullanıcı authentication - kayıt/giriş olmadan uygulama kullanımı
    /// </summary>
    /// <param name="requestDto">Device ID ve cihaz bilgileri</param>
    /// <returns>Anonim kullanıcı JWT token (24 saat geçerli)</returns>
    /// <response code="200">Anonim token başarıyla oluşturuldu</response>
    /// <response code="400">Geçersiz device ID</response>
    /// <response code="500">Sunucu hatası</response>
    /// <remarks>
    /// Bu endpoint mobil/web uygulamanın kayıt olmadan kullanılabilmesi için gereklidir.
    /// Device ID benzersiz olmalı ve cihaz/browser başına bir tane olmalıdır.
    /// 
    /// Örnek kullanım:
    /// - iOS: UIDevice.identifierForVendor
    /// - Android: Settings.Secure.ANDROID_ID veya custom UUID
    /// - Web: localStorage'da saklanacak UUID
    /// </remarks>
    [HttpPost("anonymous")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status500InternalServerError)]
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

    /// <summary>
    /// Mevcut kullanıcının bilgilerini getir
    /// </summary>
    /// <returns>Kullanıcı bilgileri</returns>
    /// <response code="200">Kullanıcı bilgileri başarıyla getirildi</response>
    /// <response code="401">Geçersiz veya süresi dolmuş token</response>
    /// <response code="404">Kullanıcı bulunamadı</response>
    /// <response code="500">Sunucu hatası</response>
    /// <remarks>
    /// Bu endpoint hem registered hem de anonymous kullanıcılar için çalışır.
    /// Authorization header'ında Bearer token gereklidir.
    /// 
    /// Header örneği:
    /// Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
    /// </remarks>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<UserResponseDto>>> GetCurrentUser()
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var deviceId = User.FindFirst("device_id")?.Value;
            var userType = User.FindFirst("user_type")?.Value;

            _logger.LogInformation($"GetCurrentUser - UserId: {userId}, DeviceId: {deviceId}, UserType: {userType}");

            if (string.IsNullOrEmpty(userId) && string.IsNullOrEmpty(deviceId))
            {
                return Unauthorized(ApiResponse<UserResponseDto>.ErrorResult("Geçersiz token"));
            }

            UserResponseDto? user = null;

            // Anonymous user kontrolü
            if (userType == "anonymous" && !string.IsNullOrEmpty(deviceId))
            {
                _logger.LogInformation($"Processing anonymous user with deviceId: {deviceId}");
                user = await _userService.GetOrCreateAnonymousAsync(deviceId);
            }
            // Registered user kontrolü
            else if (!string.IsNullOrEmpty(userId))
            {
                _logger.LogInformation($"Processing registered user with userId: {userId}");
                user = await _userService.GetByIdAsync(userId);
                
                if (user == null)
                {
                    _logger.LogWarning($"Registered user not found in database: {userId}");
                }
            }

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

    /// <summary>
    /// JWT token yenileme - süresi dolmuş access token'ları yeniler
    /// </summary>
    /// <param name="request">Refresh token</param>
    /// <returns>Yeni JWT token seti</returns>
    /// <response code="200">Token başarıyla yenilendi</response>
    /// <response code="401">Geçersiz, süresi dolmuş veya kullanılmış refresh token</response>
    /// <response code="500">Sunucu hatası</response>
    /// <remarks>
    /// Refresh token ile yeni access token alabilirsiniz.
    /// 
    /// **Token Rotation:** Güvenlik için her refresh işleminde hem access hem refresh token yenilenir.
    /// 
    /// **Geçerlilik Süreleri:**
    /// - Access Token: 60 dakika
    /// - Refresh Token: 30 gün
    /// 
    /// **Kullanım:**
    /// 1. Access token süresi dolduğunda 401 hatası alırsınız
    /// 2. Bu endpoint ile refresh token göndererek yeni token alın
    /// 3. Eski refresh token geçersiz hale gelir
    /// 4. Yeni token seti ile API kullanımına devam edin
    /// </remarks>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        try
        {
            var result = await _userService.RefreshTokenAsync(request.RefreshToken);
            return Ok(ApiResponse<AuthResponseDto>.SuccessResult(result, "Token yenilendi"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<AuthResponseDto>.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RefreshToken error");
            return StatusCode(500, ApiResponse<AuthResponseDto>.ErrorResult("Bir hata oluştu"));
        }
    }

    /// <summary>
    /// Kullanıcı çıkış işlemi
    /// </summary>
    /// <returns>Çıkış başarı mesajı</returns>
    /// <response code="200">Çıkış başarılı</response>
    /// <response code="401">Geçersiz token</response>
    /// <response code="500">Sunucu hatası</response>
    /// <remarks>
    /// JWT token'lar stateless olduğu için server-side logout yoktur.
    /// Client-side'da token'ı silmek yeterlidir.
    /// 
    /// Bu endpoint gelecekte token blacklist özelliği için kullanılabilir.
    /// 
    /// Client tarafında yapılması gerekenler:
    /// - localStorage/sessionStorage'dan token'ı sil
    /// - Memory'deki token referanslarını temizle
    /// - Login sayfasına yönlendir
    /// </remarks>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
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
