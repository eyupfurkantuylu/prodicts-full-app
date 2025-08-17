using Application.Interface;
using Application.Models.DTOs;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

/// <summary>
/// FlashCard yönetimi için endpoint'ler
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("FlashCard")]
[Authorize]
public class FlashCardController : ControllerBase
{
    private readonly IFlashCardService _flashCardService;
    private readonly ILogger<FlashCardController> _logger;

    public FlashCardController(
        IFlashCardService flashCardService,
        ILogger<FlashCardController> logger)
    {
        _flashCardService = flashCardService;
        _logger = logger;
    }

    /// <summary>
    /// Kullanıcının tüm flashcard'larını getir
    /// </summary>
    /// <returns>Kullanıcının flashcard'ları</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<FlashCardResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<FlashCardResponseDto>>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<IEnumerable<FlashCardResponseDto>>>> GetAll()
    {
        try
        {
            var userId = GetUserId();
            var flashCards = await _flashCardService.GetByUserIdAsync(userId);
            return Ok(ApiResponse<IEnumerable<FlashCardResponseDto>>.SuccessResult(flashCards));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetAll flashcards error");
            return StatusCode(500, ApiResponse<IEnumerable<FlashCardResponseDto>>.ErrorResult("Bir hata oluştu"));
        }
    }

    /// <summary>
    /// Belirli grup ID'sine ait flashcard'ları getir
    /// </summary>
    /// <param name="groupId">Grup ID</param>
    /// <returns>Gruba ait flashcard'lar</returns>
    [HttpGet("group/{groupId}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<FlashCardResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<FlashCardResponseDto>>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<IEnumerable<FlashCardResponseDto>>>> GetByGroupId(string groupId)
    {
        try
        {
            var userId = GetUserId();
            var flashCards = await _flashCardService.GetByGroupIdAsync(groupId, userId);
            return Ok(ApiResponse<IEnumerable<FlashCardResponseDto>>.SuccessResult(flashCards));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetByGroupId flashcards error");
            return StatusCode(500, ApiResponse<IEnumerable<FlashCardResponseDto>>.ErrorResult("Bir hata oluştu"));
        }
    }

    /// <summary>
    /// Tekrar edilmeyi bekleyen flashcard'ları getir
    /// </summary>
    /// <returns>Tekrar edilecek flashcard'lar</returns>
    [HttpGet("due")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<FlashCardResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<FlashCardResponseDto>>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<IEnumerable<FlashCardResponseDto>>>> GetDueForReview()
    {
        try
        {
            var userId = GetUserId();
            var flashCards = await _flashCardService.GetDueForReviewAsync(userId);
            return Ok(ApiResponse<IEnumerable<FlashCardResponseDto>>.SuccessResult(flashCards, "Tekrar edilecek kartlar getirildi"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetDueForReview flashcards error");
            return StatusCode(500, ApiResponse<IEnumerable<FlashCardResponseDto>>.ErrorResult("Bir hata oluştu"));
        }
    }

    /// <summary>
    /// ID'ye göre flashcard getir
    /// </summary>
    /// <param name="id">FlashCard ID</param>
    /// <returns>FlashCard bilgileri</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<FlashCardResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<FlashCardResponseDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<FlashCardResponseDto>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<FlashCardResponseDto>>> GetById(string id)
    {
        try
        {
            var userId = GetUserId();
            var flashCard = await _flashCardService.GetByIdAsync(id, userId);
            
            if (flashCard == null)
                return NotFound(ApiResponse<FlashCardResponseDto>.ErrorResult("FlashCard bulunamadı"));

            return Ok(ApiResponse<FlashCardResponseDto>.SuccessResult(flashCard));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetById flashcard error");
            return StatusCode(500, ApiResponse<FlashCardResponseDto>.ErrorResult("Bir hata oluştu"));
        }
    }

    /// <summary>
    /// Yeni flashcard oluştur
    /// </summary>
    /// <param name="dto">FlashCard bilgileri</param>
    /// <returns>Oluşturulan flashcard</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<FlashCardResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<FlashCardResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<FlashCardResponseDto>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<FlashCardResponseDto>>> Create([FromBody] CreateFlashCardDto dto)
    {
        try
        {
            var userId = GetUserId();
            var flashCard = await _flashCardService.CreateAsync(dto, userId);
            return CreatedAtAction(nameof(GetById), new { id = flashCard.Id }, 
                ApiResponse<FlashCardResponseDto>.SuccessResult(flashCard, "FlashCard oluşturuldu"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return BadRequest(ApiResponse<FlashCardResponseDto>.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Create flashcard error");
            return StatusCode(500, ApiResponse<FlashCardResponseDto>.ErrorResult("Bir hata oluştu"));
        }
    }

    /// <summary>
    /// FlashCard güncelle
    /// </summary>
    /// <param name="id">FlashCard ID</param>
    /// <param name="dto">Güncellenecek bilgiler</param>
    /// <returns>Güncellenmiş flashcard</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<FlashCardResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<FlashCardResponseDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<FlashCardResponseDto>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<FlashCardResponseDto>>> Update(string id, [FromBody] UpdateFlashCardDto dto)
    {
        try
        {
            var userId = GetUserId();
            var flashCard = await _flashCardService.UpdateAsync(id, dto, userId);
            
            if (flashCard == null)
                return NotFound(ApiResponse<FlashCardResponseDto>.ErrorResult("FlashCard bulunamadı"));

            return Ok(ApiResponse<FlashCardResponseDto>.SuccessResult(flashCard, "FlashCard güncellendi"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Update flashcard error");
            return StatusCode(500, ApiResponse<FlashCardResponseDto>.ErrorResult("Bir hata oluştu"));
        }
    }

    /// <summary>
    /// FlashCard sil
    /// </summary>
    /// <param name="id">FlashCard ID</param>
    /// <returns>Silme durumu</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse>> Delete(string id)
    {
        try
        {
            var userId = GetUserId();
            var result = await _flashCardService.DeleteAsync(id, userId);
            
            if (!result)
                return NotFound(ApiResponse.ErrorResult("FlashCard bulunamadı"));

            return Ok(ApiResponse.SuccessResult("FlashCard silindi"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Delete flashcard error");
            return StatusCode(500, ApiResponse.ErrorResult("Bir hata oluştu"));
        }
    }

    /// <summary>
    /// FlashCard tekrar işlemi
    /// </summary>
    /// <param name="id">FlashCard ID</param>
    /// <param name="dto">Tekrar sonucu (doğru/yanlış)</param>
    /// <returns>Güncellenmiş flashcard</returns>
    [HttpPost("{id}/review")]
    [ProducesResponseType(typeof(ApiResponse<FlashCardResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<FlashCardResponseDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<FlashCardResponseDto>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<FlashCardResponseDto>>> Review(string id, [FromBody] ReviewFlashCardDto dto)
    {
        try
        {
            var userId = GetUserId();
            var flashCard = await _flashCardService.ReviewAsync(id, dto, userId);
            
            if (flashCard == null)
                return NotFound(ApiResponse<FlashCardResponseDto>.ErrorResult("FlashCard bulunamadı"));

            return Ok(ApiResponse<FlashCardResponseDto>.SuccessResult(flashCard, "Tekrar kaydedildi"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Review flashcard error");
            return StatusCode(500, ApiResponse<FlashCardResponseDto>.ErrorResult("Bir hata oluştu"));
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
