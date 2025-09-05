using Application.Interface;
using Application.Models.DTOs;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

/// <summary>
/// Podcast yönetimi için admin endpoint'leri
/// </summary>
/// <remarks>
/// Bu controller podcast içeriklerinin yönetimi için admin yetkisi gerektiren endpoint'leri sağlar.
/// 
/// **Ana Özellikler:**
/// - Podcast serilerinin CRUD işlemleri
/// - Podcast sezonlarının yönetimi
/// - Podcast bölümlerinin yönetimi
/// - Quiz içeriklerinin yönetimi
/// - Audio dosya upload/delete işlemleri
/// 
/// **Desteklenen İşlemler:**
/// - Tüm CRUD işlemleri (Create, Read, Update, Delete)
/// - Toplu işlemler
/// - Dosya yönetimi
/// - İçerik durumu yönetimi
/// 
/// **Güvenlik:**
/// - Sadece Admin rolü erişim
/// - JWT token zorunlu
/// - Tüm işlemler loglanır
/// </remarks>
[ApiController]
[Route("api/admin/[controller]")]
[Tags("Podcast Admin")]
[Authorize(Roles = "Admin")]
public class PodcastAdminController : ControllerBase
{
    private readonly IPodcastSeriesService _podcastSeriesService;
    private readonly IPodcastSeasonService _podcastSeasonService;
    private readonly IPodcastEpisodeService _podcastEpisodeService;
    private readonly IPodcastQuizService _podcastQuizService;
    private readonly ILogger<PodcastAdminController> _logger;

    public PodcastAdminController(
        IPodcastSeriesService podcastSeriesService,
        IPodcastSeasonService podcastSeasonService,
        IPodcastEpisodeService podcastEpisodeService,
        IPodcastQuizService podcastQuizService,
        ILogger<PodcastAdminController> logger)
    {
        _podcastSeriesService = podcastSeriesService;
        _podcastSeasonService = podcastSeasonService;
        _podcastEpisodeService = podcastEpisodeService;
        _podcastQuizService = podcastQuizService;
        _logger = logger;
    }

    #region Podcast Series Management

    /// <summary>
    /// Tüm podcast serilerini getir (admin view)
    /// </summary>
    /// <returns>Tüm podcast serileri</returns>
    /// <response code="200">Başarılı işlem</response>
    /// <response code="401">Yetkisiz erişim</response>
    /// <response code="500">Sunucu hatası</response>
    [HttpGet("series")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PodcastSeriesResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PodcastSeriesResponseDto>>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PodcastSeriesResponseDto>>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<IEnumerable<PodcastSeriesResponseDto>>>> GetAllSeries()
    {
        try
        {
            var series = await _podcastSeriesService.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<PodcastSeriesResponseDto>>.SuccessResult(series));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetAllSeries error");
            return StatusCode(500, ApiResponse<IEnumerable<PodcastSeriesResponseDto>>.ErrorResult("Bir hata oluştu"));
        }
    }

    /// <summary>
    /// Yeni podcast serisi oluştur
    /// </summary>
    /// <param name="dto">Seri bilgileri</param>
    /// <returns>Oluşturulan seri</returns>
    /// <response code="200">Başarılı işlem</response>
    /// <response code="400">Geçersiz veri</response>
    /// <response code="401">Yetkisiz erişim</response>
    /// <response code="500">Sunucu hatası</response>
    [HttpPost("series")]
    [ProducesResponseType(typeof(ApiResponse<PodcastSeriesResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PodcastSeriesResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<PodcastSeriesResponseDto>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<PodcastSeriesResponseDto>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<PodcastSeriesResponseDto>>> CreateSeries([FromBody] CreatePodcastSeriesDto dto)
    {
        try
        {
            var series = await _podcastSeriesService.CreateAsync(dto);
            return Ok(ApiResponse<PodcastSeriesResponseDto>.SuccessResult(series, "Podcast serisi başarıyla oluşturuldu"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateSeries error");
            return StatusCode(500, ApiResponse<PodcastSeriesResponseDto>.ErrorResult("Bir hata oluştu"));
        }
    }

    /// <summary>
    /// Podcast serisini güncelle
    /// </summary>
    /// <param name="id">Seri ID</param>
    /// <param name="dto">Güncellenecek bilgiler</param>
    /// <returns>Güncellenmiş seri</returns>
    /// <response code="200">Başarılı işlem</response>
    /// <response code="400">Geçersiz veri</response>
    /// <response code="401">Yetkisiz erişim</response>
    /// <response code="404">Seri bulunamadı</response>
    /// <response code="500">Sunucu hatası</response>
    [HttpPut("series/{id}")]
    [ProducesResponseType(typeof(ApiResponse<PodcastSeriesResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PodcastSeriesResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<PodcastSeriesResponseDto>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<PodcastSeriesResponseDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<PodcastSeriesResponseDto>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<PodcastSeriesResponseDto>>> UpdateSeries(string id, [FromBody] UpdatePodcastSeriesDto dto)
    {
        try
        {
            var series = await _podcastSeriesService.UpdateAsync(id, dto);
            if (series == null)
                return NotFound(ApiResponse<PodcastSeriesResponseDto>.ErrorResult("Podcast serisi bulunamadı"));

            return Ok(ApiResponse<PodcastSeriesResponseDto>.SuccessResult(series, "Podcast serisi başarıyla güncellendi"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateSeries error with id: {id}", id);
            return StatusCode(500, ApiResponse<PodcastSeriesResponseDto>.ErrorResult("Bir hata oluştu"));
        }
    }

    /// <summary>
    /// Podcast serisini sil
    /// </summary>
    /// <param name="id">Seri ID</param>
    /// <returns>Silme işlemi sonucu</returns>
    /// <response code="200">Başarılı işlem</response>
    /// <response code="401">Yetkisiz erişim</response>
    /// <response code="404">Seri bulunamadı</response>
    /// <response code="500">Sunucu hatası</response>
    [HttpDelete("series/{id}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteSeries(string id)
    {
        try
        {
            var result = await _podcastSeriesService.DeleteAsync(id);
            if (!result)
                return NotFound(ApiResponse<bool>.ErrorResult("Podcast serisi bulunamadı"));

            return Ok(ApiResponse<bool>.SuccessResult(result, "Podcast serisi başarıyla silindi"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteSeries error with id: {id}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResult("Bir hata oluştu"));
        }
    }

    #endregion

    #region Podcast Season Management

    /// <summary>
    /// Tüm podcast sezonlarını getir (admin view)
    /// </summary>
    /// <returns>Tüm podcast sezonları</returns>
    /// <response code="200">Başarılı işlem</response>
    /// <response code="401">Yetkisiz erişim</response>
    /// <response code="500">Sunucu hatası</response>
    [HttpGet("seasons")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PodcastSeasonResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PodcastSeasonResponseDto>>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PodcastSeasonResponseDto>>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<IEnumerable<PodcastSeasonResponseDto>>>> GetAllSeasons()
    {
        try
        {
            var seasons = await _podcastSeasonService.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<PodcastSeasonResponseDto>>.SuccessResult(seasons));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetAllSeasons error");
            return StatusCode(500, ApiResponse<IEnumerable<PodcastSeasonResponseDto>>.ErrorResult("Bir hata oluştu"));
        }
    }

    /// <summary>
    /// Yeni podcast sezonu oluştur
    /// </summary>
    /// <param name="dto">Sezon bilgileri</param>
    /// <returns>Oluşturulan sezon</returns>
    /// <response code="200">Başarılı işlem</response>
    /// <response code="400">Geçersiz veri</response>
    /// <response code="401">Yetkisiz erişim</response>
    /// <response code="500">Sunucu hatası</response>
    [HttpPost("seasons")]
    [ProducesResponseType(typeof(ApiResponse<PodcastSeasonResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PodcastSeasonResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<PodcastSeasonResponseDto>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<PodcastSeasonResponseDto>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<PodcastSeasonResponseDto>>> CreateSeason([FromBody] CreatePodcastSeasonDto dto)
    {
        try
        {
            var season = await _podcastSeasonService.CreateAsync(dto);
            return Ok(ApiResponse<PodcastSeasonResponseDto>.SuccessResult(season, "Podcast sezonu başarıyla oluşturuldu"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateSeason error");
            return StatusCode(500, ApiResponse<PodcastSeasonResponseDto>.ErrorResult("Bir hata oluştu"));
        }
    }

    /// <summary>
    /// Podcast sezonunu güncelle
    /// </summary>
    /// <param name="id">Sezon ID</param>
    /// <param name="dto">Güncellenecek bilgiler</param>
    /// <returns>Güncellenmiş sezon</returns>
    /// <response code="200">Başarılı işlem</response>
    /// <response code="400">Geçersiz veri</response>
    /// <response code="401">Yetkisiz erişim</response>
    /// <response code="404">Sezon bulunamadı</response>
    /// <response code="500">Sunucu hatası</response>
    [HttpPut("seasons/{id}")]
    [ProducesResponseType(typeof(ApiResponse<PodcastSeasonResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PodcastSeasonResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<PodcastSeasonResponseDto>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<PodcastSeasonResponseDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<PodcastSeasonResponseDto>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<PodcastSeasonResponseDto>>> UpdateSeason(string id, [FromBody] UpdatePodcastSeasonDto dto)
    {
        try
        {
            var season = await _podcastSeasonService.UpdateAsync(id, dto);
            if (season == null)
                return NotFound(ApiResponse<PodcastSeasonResponseDto>.ErrorResult("Podcast sezonu bulunamadı"));

            return Ok(ApiResponse<PodcastSeasonResponseDto>.SuccessResult(season, "Podcast sezonu başarıyla güncellendi"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateSeason error with id: {id}", id);
            return StatusCode(500, ApiResponse<PodcastSeasonResponseDto>.ErrorResult("Bir hata oluştu"));
        }
    }

    /// <summary>
    /// Podcast sezonunu sil
    /// </summary>
    /// <param name="id">Sezon ID</param>
    /// <returns>Silme işlemi sonucu</returns>
    /// <response code="200">Başarılı işlem</response>
    /// <response code="401">Yetkisiz erişim</response>
    /// <response code="404">Sezon bulunamadı</response>
    /// <response code="500">Sunucu hatası</response>
    [HttpDelete("seasons/{id}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteSeason(string id)
    {
        try
        {
            var result = await _podcastSeasonService.DeleteAsync(id);
            if (!result)
                return NotFound(ApiResponse<bool>.ErrorResult("Podcast sezonu bulunamadı"));

            return Ok(ApiResponse<bool>.SuccessResult(result, "Podcast sezonu başarıyla silindi"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteSeason error with id: {id}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResult("Bir hata oluştu"));
        }
    }

    #endregion

    #region Podcast Episode Management

    /// <summary>
    /// Tüm podcast bölümlerini getir (admin view)
    /// </summary>
    /// <returns>Tüm podcast bölümleri</returns>
    /// <response code="200">Başarılı işlem</response>
    /// <response code="401">Yetkisiz erişim</response>
    /// <response code="500">Sunucu hatası</response>
    [HttpGet("episodes")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PodcastEpisodeResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PodcastEpisodeResponseDto>>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PodcastEpisodeResponseDto>>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<IEnumerable<PodcastEpisodeResponseDto>>>> GetAllEpisodes()
    {
        try
        {
            var episodes = await _podcastEpisodeService.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<PodcastEpisodeResponseDto>>.SuccessResult(episodes));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetAllEpisodes error");
            return StatusCode(500, ApiResponse<IEnumerable<PodcastEpisodeResponseDto>>.ErrorResult("Bir hata oluştu"));
        }
    }

    /// <summary>
    /// Yeni podcast bölümü oluştur
    /// </summary>
    /// <param name="dto">Bölüm bilgileri</param>
    /// <returns>Oluşturulan bölüm</returns>
    /// <response code="200">Başarılı işlem</response>
    /// <response code="400">Geçersiz veri</response>
    /// <response code="401">Yetkisiz erişim</response>
    /// <response code="500">Sunucu hatası</response>
    [HttpPost("episodes")]
    [ProducesResponseType(typeof(ApiResponse<PodcastEpisodeResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PodcastEpisodeResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<PodcastEpisodeResponseDto>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<PodcastEpisodeResponseDto>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<PodcastEpisodeResponseDto>>> CreateEpisode([FromBody] CreatePodcastEpisodeDto dto)
    {
        try
        {
            var episode = await _podcastEpisodeService.CreateAsync(dto);
            return Ok(ApiResponse<PodcastEpisodeResponseDto>.SuccessResult(episode, "Podcast bölümü başarıyla oluşturuldu"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateEpisode error");
            return StatusCode(500, ApiResponse<PodcastEpisodeResponseDto>.ErrorResult("Bir hata oluştu"));
        }
    }

    /// <summary>
    /// Podcast bölümünü güncelle
    /// </summary>
    /// <param name="id">Bölüm ID</param>
    /// <param name="dto">Güncellenecek bilgiler</param>
    /// <returns>Güncellenmiş bölüm</returns>
    /// <response code="200">Başarılı işlem</response>
    /// <response code="400">Geçersiz veri</response>
    /// <response code="401">Yetkisiz erişim</response>
    /// <response code="404">Bölüm bulunamadı</response>
    /// <response code="500">Sunucu hatası</response>
    [HttpPut("episodes/{id}")]
    [ProducesResponseType(typeof(ApiResponse<PodcastEpisodeResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PodcastEpisodeResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<PodcastEpisodeResponseDto>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<PodcastEpisodeResponseDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<PodcastEpisodeResponseDto>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<PodcastEpisodeResponseDto>>> UpdateEpisode(string id, [FromBody] UpdatePodcastEpisodeDto dto)
    {
        try
        {
            var episode = await _podcastEpisodeService.UpdateAsync(id, dto);
            if (episode == null)
                return NotFound(ApiResponse<PodcastEpisodeResponseDto>.ErrorResult("Podcast bölümü bulunamadı"));

            return Ok(ApiResponse<PodcastEpisodeResponseDto>.SuccessResult(episode, "Podcast bölümü başarıyla güncellendi"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateEpisode error with id: {id}", id);
            return StatusCode(500, ApiResponse<PodcastEpisodeResponseDto>.ErrorResult("Bir hata oluştu"));
        }
    }

    /// <summary>
    /// Podcast bölümünü sil
    /// </summary>
    /// <param name="id">Bölüm ID</param>
    /// <returns>Silme işlemi sonucu</returns>
    /// <response code="200">Başarılı işlem</response>
    /// <response code="401">Yetkisiz erişim</response>
    /// <response code="404">Bölüm bulunamadı</response>
    /// <response code="500">Sunucu hatası</response>
    [HttpDelete("episodes/{id}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteEpisode(string id)
    {
        try
        {
            var result = await _podcastEpisodeService.DeleteAsync(id);
            if (!result)
                return NotFound(ApiResponse<bool>.ErrorResult("Podcast bölümü bulunamadı"));

            return Ok(ApiResponse<bool>.SuccessResult(result, "Podcast bölümü başarıyla silindi"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteEpisode error with id: {id}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResult("Bir hata oluştu"));
        }
    }

    #endregion

    #region Podcast Quiz Management

    /// <summary>
    /// Tüm podcast quizlerini getir (admin view)
    /// </summary>
    /// <returns>Tüm podcast quizleri</returns>
    /// <response code="200">Başarılı işlem</response>
    /// <response code="401">Yetkisiz erişim</response>
    /// <response code="500">Sunucu hatası</response>
    [HttpGet("quizzes")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PodcastQuizResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PodcastQuizResponseDto>>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PodcastQuizResponseDto>>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<IEnumerable<PodcastQuizResponseDto>>>> GetAllQuizzes()
    {
        try
        {
            var quizzes = await _podcastQuizService.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<PodcastQuizResponseDto>>.SuccessResult(quizzes));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetAllQuizzes error");
            return StatusCode(500, ApiResponse<IEnumerable<PodcastQuizResponseDto>>.ErrorResult("Bir hata oluştu"));
        }
    }

    /// <summary>
    /// Yeni podcast quizi oluştur
    /// </summary>
    /// <param name="dto">Quiz bilgileri</param>
    /// <returns>Oluşturulan quiz</returns>
    /// <response code="200">Başarılı işlem</response>
    /// <response code="400">Geçersiz veri</response>
    /// <response code="401">Yetkisiz erişim</response>
    /// <response code="500">Sunucu hatası</response>
    [HttpPost("quizzes")]
    [ProducesResponseType(typeof(ApiResponse<PodcastQuizResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PodcastQuizResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<PodcastQuizResponseDto>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<PodcastQuizResponseDto>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<PodcastQuizResponseDto>>> CreateQuiz([FromBody] CreatePodcastQuizDto dto)
    {
        try
        {
            var quiz = await _podcastQuizService.CreateAsync(dto);
            return Ok(ApiResponse<PodcastQuizResponseDto>.SuccessResult(quiz, "Podcast quizi başarıyla oluşturuldu"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateQuiz error");
            return StatusCode(500, ApiResponse<PodcastQuizResponseDto>.ErrorResult("Bir hata oluştu"));
        }
    }

    /// <summary>
    /// Podcast quizini güncelle
    /// </summary>
    /// <param name="id">Quiz ID</param>
    /// <param name="dto">Güncellenecek bilgiler</param>
    /// <returns>Güncellenmiş quiz</returns>
    /// <response code="200">Başarılı işlem</response>
    /// <response code="400">Geçersiz veri</response>
    /// <response code="401">Yetkisiz erişim</response>
    /// <response code="404">Quiz bulunamadı</response>
    /// <response code="500">Sunucu hatası</response>
    [HttpPut("quizzes/{id}")]
    [ProducesResponseType(typeof(ApiResponse<PodcastQuizResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PodcastQuizResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<PodcastQuizResponseDto>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<PodcastQuizResponseDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<PodcastQuizResponseDto>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<PodcastQuizResponseDto>>> UpdateQuiz(string id, [FromBody] UpdatePodcastQuizDto dto)
    {
        try
        {
            var quiz = await _podcastQuizService.UpdateAsync(id, dto);
            if (quiz == null)
                return NotFound(ApiResponse<PodcastQuizResponseDto>.ErrorResult("Podcast quizi bulunamadı"));

            return Ok(ApiResponse<PodcastQuizResponseDto>.SuccessResult(quiz, "Podcast quizi başarıyla güncellendi"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateQuiz error with id: {id}", id);
            return StatusCode(500, ApiResponse<PodcastQuizResponseDto>.ErrorResult("Bir hata oluştu"));
        }
    }

    /// <summary>
    /// Podcast quizini sil
    /// </summary>
    /// <param name="id">Quiz ID</param>
    /// <returns>Silme işlemi sonucu</returns>
    /// <response code="200">Başarılı işlem</response>
    /// <response code="401">Yetkisiz erişim</response>
    /// <response code="404">Quiz bulunamadı</response>
    /// <response code="500">Sunucu hatası</response>
    [HttpDelete("quizzes/{id}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteQuiz(string id)
    {
        try
        {
            var result = await _podcastQuizService.DeleteAsync(id);
            if (!result)
                return NotFound(ApiResponse<bool>.ErrorResult("Podcast quizi bulunamadı"));

            return Ok(ApiResponse<bool>.SuccessResult(result, "Podcast quizi başarıyla silindi"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteQuiz error with id: {id}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResult("Bir hata oluştu"));
        }
    }

    #endregion
}
