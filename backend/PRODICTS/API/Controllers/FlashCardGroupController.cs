using Application.Interface;
using Application.Models.DTOs;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

/// <summary>
/// FlashCard Group yönetimi için endpoint'ler
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("FlashCardGroup")]
[Authorize]
public class FlashCardGroupController : ControllerBase
{
    private readonly IFlashCardGroupService _flashCardGroupService;
    private readonly ILogger<FlashCardGroupController> _logger;

    public FlashCardGroupController(
        IFlashCardGroupService flashCardGroupService,
        ILogger<FlashCardGroupController> logger)
    {
        _flashCardGroupService = flashCardGroupService;
        _logger = logger;
    }

    /// <summary>
    /// Kullanıcının tüm flashcard gruplarını getir
    /// </summary>
    /// <returns>Kullanıcının flashcard grupları</returns>
    [HttpGet]
    [Authorize(Roles = "Admin, User")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<FlashCardGroupResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<FlashCardGroupResponseDto>>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<IEnumerable<FlashCardGroupResponseDto>>>> GetAll()
    {
        try
        {
            var userId = GetUserId();
            var groups = await _flashCardGroupService.GetByUserIdAsync(userId);
            return Ok(ApiResponse<IEnumerable<FlashCardGroupResponseDto>>.SuccessResult(groups));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetAll flashcard groups error");
            return StatusCode(500, ApiResponse<IEnumerable<FlashCardGroupResponseDto>>.ErrorResult("Bir hata oluştu"));
        }
    }

    /// <summary>
    /// ID'ye göre flashcard grubu getir
    /// </summary>
    /// <param name="id">FlashCard Group ID</param>
    /// <returns>FlashCard Group bilgileri</returns>
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin, User")]
    [ProducesResponseType(typeof(ApiResponse<FlashCardGroupResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<FlashCardGroupResponseDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<FlashCardGroupResponseDto>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<FlashCardGroupResponseDto>>> GetById(string id)
    {
        try
        {
            var userId = GetUserId();
            var group = await _flashCardGroupService.GetByIdAsync(id, userId);
            
            if (group == null)
                return NotFound(ApiResponse<FlashCardGroupResponseDto>.ErrorResult("FlashCard grubu bulunamadı"));

            return Ok(ApiResponse<FlashCardGroupResponseDto>.SuccessResult(group));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetById flashcard group error");
            return StatusCode(500, ApiResponse<FlashCardGroupResponseDto>.ErrorResult("Bir hata oluştu"));
        }
    }

    /// <summary>
    /// Yeni flashcard grubu oluştur
    /// </summary>
    /// <param name="dto">FlashCard Group bilgileri</param>
    /// <returns>Oluşturulan flashcard grubu</returns>
    [HttpPost]
    [Authorize(Roles = "Admin, User")]
    [ProducesResponseType(typeof(ApiResponse<FlashCardGroupResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<FlashCardGroupResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<FlashCardGroupResponseDto>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<FlashCardGroupResponseDto>>> Create([FromBody] CreateFlashCardGroupDto dto)
    {
        try
        {
            var userId = GetUserId();
            var group = await _flashCardGroupService.CreateAsync(dto, userId);
            return CreatedAtAction(nameof(GetById), new { id = group.Id }, 
                ApiResponse<FlashCardGroupResponseDto>.SuccessResult(group, "FlashCard grubu oluşturuldu"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<FlashCardGroupResponseDto>.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Create flashcard group error");
            return StatusCode(500, ApiResponse<FlashCardGroupResponseDto>.ErrorResult("Bir hata oluştu"));
        }
    }

    /// <summary>
    /// FlashCard grubu güncelle
    /// </summary>
    /// <param name="id">FlashCard Group ID</param>
    /// <param name="dto">Güncellenecek bilgiler</param>
    /// <returns>Güncellenmiş flashcard grubu</returns>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin, User")]
    [ProducesResponseType(typeof(ApiResponse<FlashCardGroupResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<FlashCardGroupResponseDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<FlashCardGroupResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<FlashCardGroupResponseDto>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<FlashCardGroupResponseDto>>> Update(string id, [FromBody] UpdateFlashCardGroupDto dto)
    {
        try
        {
            var userId = GetUserId();
            var group = await _flashCardGroupService.UpdateAsync(id, dto, userId);
            
            if (group == null)
                return NotFound(ApiResponse<FlashCardGroupResponseDto>.ErrorResult("FlashCard grubu bulunamadı"));

            return Ok(ApiResponse<FlashCardGroupResponseDto>.SuccessResult(group, "FlashCard grubu güncellendi"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<FlashCardGroupResponseDto>.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Update flashcard group error");
            return StatusCode(500, ApiResponse<FlashCardGroupResponseDto>.ErrorResult("Bir hata oluştu"));
        }
    }

    /// <summary>
    /// FlashCard grubu sil
    /// </summary>
    /// <param name="id">FlashCard Group ID</param>
    /// <returns>Silme durumu</returns>
    /// <remarks>
    /// Grup silindiğinde, gruba ait tüm flashcard'lar da silinir.
    /// </remarks>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin, User")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse>> Delete(string id)
    {
        try
        {
            var userId = GetUserId();
            var result = await _flashCardGroupService.DeleteAsync(id, userId);
            
            if (!result)
                return NotFound(ApiResponse.ErrorResult("FlashCard grubu bulunamadı"));

            return Ok(ApiResponse.SuccessResult("FlashCard grubu ve tüm kartları silindi"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Delete flashcard group error");
            return StatusCode(500, ApiResponse.ErrorResult("Bir hata oluştu"));
        }
    }

    private string GetUserId()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var deviceId = User.FindFirst("device_id")?.Value;
        
        // Anonymous user için device ID'yi kullan
        if (!string.IsNullOrEmpty(deviceId))
            return deviceId;
        
        // Registered user için user ID'yi kullan
        if (!string.IsNullOrEmpty(userId))
            return userId;
        
        throw new UnauthorizedAccessException("Kullanıcı kimliği bulunamadı");
    }
}
