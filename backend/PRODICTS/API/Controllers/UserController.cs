using Application.Interface;
using Application.Models.DTOs;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Tüm endpoint'leri koru
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserService userService, ILogger<UserController> logger)
    {
        _userService = userService;
        _logger = logger;
    }



  

    [HttpPost("register-provider")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<UserResponseDto>>> RegisterWithProvider([FromBody] RegisterWithProviderDto providerDto)
    {
        try
        {
            var result = await _userService.RegisterWithProviderAsync(providerDto);
            return Ok(ApiResponse<UserResponseDto>.SuccessResult(result, "Provider ile kayıt başarılı"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<UserResponseDto>.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RegisterWithProvider error");
            return StatusCode(500, ApiResponse<UserResponseDto>.ErrorResult("Bir hata oluştu"));
        }
    }

    [HttpPost("anonymous")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<UserResponseDto>>> GetOrCreateAnonymous([FromBody] AnonymousUserRequestDto requestDto)
    {
        try
        {
            var result = await _userService.GetOrCreateAnonymousAsync(requestDto.DeviceId);
            return Ok(ApiResponse<UserResponseDto>.SuccessResult(result, "Anonymous user hazır"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetOrCreateAnonymous error");
            return StatusCode(500, ApiResponse<UserResponseDto>.ErrorResult("Bir hata oluştu"));
        }
    }

    [HttpPost("sync")]
    public async Task<ActionResult<ApiResponse<UserResponseDto>>> SyncUserData([FromBody] SyncUserDto syncDto)
    {
        try
        {
            var result = await _userService.SyncUserDataAsync(syncDto);
            return Ok(ApiResponse<UserResponseDto>.SuccessResult(result, "Sync başarılı"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<UserResponseDto>.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SyncUserData error");
            return StatusCode(500, ApiResponse<UserResponseDto>.ErrorResult("Bir hata oluştu"));
        }
    }

    [HttpPost("upgrade-anonymous")]
    public async Task<ActionResult<ApiResponse<UserResponseDto>>> UpgradeAnonymousToRegistered([FromBody] UpgradeAnonymousRequestDto requestDto)
    {
        try
        {
            var result = await _userService.UpgradeAnonymousToRegisteredAsync(requestDto.DeviceId, requestDto.RegisterData);
            return Ok(ApiResponse<UserResponseDto>.SuccessResult(result, "Anonymous user kayıtlı kullanıcıya dönüştürüldü"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<UserResponseDto>.ErrorResult(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<UserResponseDto>.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpgradeAnonymousToRegistered error");
            return StatusCode(500, ApiResponse<UserResponseDto>.ErrorResult("Bir hata oluştu"));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<UserResponseDto>>> GetById(string id)
    {
        try
        {
            var result = await _userService.GetByIdAsync(id);
            if (result == null)
            {
                return NotFound(ApiResponse<UserResponseDto>.ErrorResult("Kullanıcı bulunamadı"));
            }

            return Ok(ApiResponse<UserResponseDto>.SuccessResult(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetById error");
            return StatusCode(500, ApiResponse<UserResponseDto>.ErrorResult("Bir hata oluştu"));
        }
    }

    [HttpGet("email/{email}")]
    public async Task<ActionResult<ApiResponse<UserResponseDto>>> GetByEmail(string email)
    {
        try
        {
            var result = await _userService.GetByEmailAsync(email);
            if (result == null)
            {
                return NotFound(ApiResponse<UserResponseDto>.ErrorResult("Kullanıcı bulunamadı"));
            }

            return Ok(ApiResponse<UserResponseDto>.SuccessResult(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetByEmail error");
            return StatusCode(500, ApiResponse<UserResponseDto>.ErrorResult("Bir hata oluştu"));
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<UserResponseDto>>> Update(string id, [FromBody] UpdateUserDto updateDto)
    {
        try
        {
            var result = await _userService.UpdateAsync(id, updateDto);
            return Ok(ApiResponse<UserResponseDto>.SuccessResult(result, "Güncelleme başarılı"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<UserResponseDto>.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Update error");
            return StatusCode(500, ApiResponse<UserResponseDto>.ErrorResult("Bir hata oluştu"));
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> Delete(string id)
    {
        try
        {
            var result = await _userService.DeleteAsync(id);
            if (!result)
            {
                return NotFound(ApiResponse.ErrorResult("Kullanıcı bulunamadı"));
            }

            return Ok(ApiResponse.SuccessResult("Kullanıcı silindi"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Delete error");
            return StatusCode(500, ApiResponse.ErrorResult("Bir hata oluştu"));
        }
    }

    [HttpGet("check-email/{email}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<bool>>> CheckEmailExists(string email)
    {
        try
        {
            var exists = await _userService.EmailExistsAsync(email);
            return Ok(ApiResponse<bool>.SuccessResult(exists));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CheckEmailExists error");
            return StatusCode(500, ApiResponse<bool>.ErrorResult("Bir hata oluştu"));
        }
    }
}


