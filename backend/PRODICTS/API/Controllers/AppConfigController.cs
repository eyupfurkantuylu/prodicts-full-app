using API.Models;
using Application.Interface;
using Application.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Tags("AppConfig")]
    public class AppConfigController : ControllerBase
    {
        private readonly IAppConfigService _appConfigService;
        private readonly ILogger<AuthController> _logger;

        public AppConfigController(IAppConfigService appConfigService, ILogger<AuthController> logger)
        {
            _appConfigService = appConfigService;
            _logger = logger;
        }
        
        /// <summary>
        /// Uygulamanın tüm app config'lerini getir
        /// </summary>
        /// <returns>Tüm app config'ler </returns>
        [HttpGet("GetAll")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<AppConfigDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<AppConfigDto>>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<IEnumerable<AppConfigDto>>>> GetAll()
        {
            try
            {
                
                var appConfig = await _appConfigService.GetAllAsync();
                return Ok(ApiResponse<IEnumerable<AppConfigDto>>.SuccessResult(appConfig));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAll app config error");
                return StatusCode(500, ApiResponse<IEnumerable<AppConfigDto>>.ErrorResult("Bir hata oluştu"));
            }
        }
        
        /// <summary>
        /// ID'ye göre app config getir
        /// </summary>
        /// <param name="id">App Config ID</param>
        /// <returns>AppConfig bilgileri</returns>
        [HttpGet("GetById/{id}")]
        [Authorize(Roles = "Admin, User")]
        [ProducesResponseType(typeof(ApiResponse<AppConfigDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<AppConfigDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<AppConfigDto>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<AppConfigDto>>> GetById(string id)
        {
            try
            {
                
                var appConfig = await _appConfigService.GetByIdAsync(id);
            
                if (appConfig == null)
                    return NotFound(ApiResponse<AppConfigDto>.ErrorResult("App Config bulunamadı"));

                return Ok(ApiResponse<AppConfigDto>.SuccessResult(appConfig));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetById app config error");
                return StatusCode(500, ApiResponse<AppConfigDto>.ErrorResult("Bir hata oluştu"));
            }
        }
        
        
        /// <summary>
        /// Yeni app config oluştur
        /// </summary>
        /// <param name="dto">AppConfig bilgileri</param>
        /// <returns>Oluşturulan appconfig</returns>
        [HttpPost("Create")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<AppConfigDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<AppConfigDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<AppConfigDto>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<AppConfigDto>>> Create([FromBody] CreateAppConfigDto dto)
        {
            try
            {
                var appConfig = await _appConfigService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = appConfig.Id }, 
                    ApiResponse<AppConfigDto>.SuccessResult(appConfig, "App Config oluşturuldu"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return BadRequest(ApiResponse<AppConfigDto>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create app config error");
                return StatusCode(500, ApiResponse<AppConfigDto>.ErrorResult("Bir hata oluştu"));
            }
        }
        
        /// <summary>
        /// AppConfig güncelle
        /// </summary>
        /// <param name="id">AppConfig ID</param>
        /// <param name="dto">Güncellenecek bilgiler</param>
        /// <returns>Güncellenmiş appConfig</returns>
        [HttpPut("Update/{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<AppConfigDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<AppConfigDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<AppConfigDto>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<AppConfigDto>>> Update(string id, [FromBody] UpdateAppConfigDto dto)
        {
            try
            {
           
                var appConfig = await _appConfigService.UpdateAsync(id, dto);
            
                if (appConfig == null)
                    return NotFound(ApiResponse<AppConfigDto>.ErrorResult("AppConfig bulunamadı"));

                return Ok(ApiResponse<AppConfigDto>.SuccessResult(appConfig, "AppConfig güncellendi"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update appConfig error");
                return StatusCode(500, ApiResponse<AppConfigDto>.ErrorResult("Bir hata oluştu"));
            }
        }
        
        
        /// <summary>
        /// AppConfig sil
        /// </summary>
        /// <param name="id">AppConfig ID</param>
        /// <returns>Silme durumu</returns>
        [HttpDelete("Delete/{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse>> Delete(string id)
        {
            try
            {
                var result = await _appConfigService.DeleteAsync(id);
            
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
    }
}
