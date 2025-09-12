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
    private readonly IFileUploadService _fileUploadService;
    private readonly IQueueService _queueService;
    private readonly ILogger<PodcastAdminController> _logger;

    public PodcastAdminController(
        IPodcastSeriesService podcastSeriesService,
        IPodcastSeasonService podcastSeasonService,
        IPodcastEpisodeService podcastEpisodeService,
        IPodcastQuizService podcastQuizService,
        IFileUploadService fileUploadService,
        IQueueService queueService,
        ILogger<PodcastAdminController> logger)
    {
        _podcastSeriesService = podcastSeriesService;
        _podcastSeasonService = podcastSeasonService;
        _podcastEpisodeService = podcastEpisodeService;
        _podcastQuizService = podcastQuizService;
        _fileUploadService = fileUploadService;
        _queueService = queueService;
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
    /// Yeni podcast bölümü oluştur (sadece metadata)
    /// </summary>
    /// <param name="dto">Episode metadata bilgileri</param>
    /// <returns>Oluşturulan episode bilgileri</returns>
    /// <response code="200">Episode başarıyla oluşturuldu</response>
    /// <response code="400">Geçersiz veri</response>
    /// <response code="401">Yetkisiz erişim</response>
    /// <response code="500">Sunucu hatası</response>
    /// <remarks>
    /// Bu endpoint sadece podcast episode metadata'sını oluşturur.
    /// Audio ve thumbnail dosyaları ayrı endpoint'lerle upload edilir.
    /// 
    /// **Kullanım Akışı:**
    /// 1. Bu endpoint ile episode oluştur
    /// 2. `POST /episodes/{id}/upload-audio` ile audio dosyası upload et
    /// 3. `POST /episodes/{id}/upload-thumbnail` ile thumbnail upload et (opsiyonel)
    /// 
    /// **Başlangıç Durumu:**
    /// - ProcessingStatus: Uploaded
    /// - DurationSeconds: 0 (audio upload'tan sonra doldurulur)
    /// - AudioQualities: Boş liste (processing'den sonra doldurulur)
    /// </remarks>
    [HttpPost("episode")]
    [ProducesResponseType(typeof(ApiResponse<PodcastEpisodeResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PodcastEpisodeResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<PodcastEpisodeResponseDto>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<PodcastEpisodeResponseDto>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<PodcastEpisodeResponseDto>>> CreatePodcastEpisode(
        [FromBody] CreatePodcastEpisodeDto dto)
    {
        try
        {
            // MongoDB ObjectId format validation
            if (!MongoDB.Bson.ObjectId.TryParse(dto.PodcastSeriesId, out _))
                return BadRequest(ApiResponse<PodcastEpisodeResponseDto>.ErrorResult("Geçersiz PodcastSeriesId formatı"));
            
            if (!MongoDB.Bson.ObjectId.TryParse(dto.PodcastSeasonId, out _))
                return BadRequest(ApiResponse<PodcastEpisodeResponseDto>.ErrorResult("Geçersiz PodcastSeasonId formatı"));

            var episode = await _podcastEpisodeService.CreateAsync(dto);
            if (episode == null)
                return BadRequest(ApiResponse<PodcastEpisodeResponseDto>.ErrorResult("Episode oluşturulamadı"));

            _logger.LogInformation("Podcast episode created. EpisodeId: {EpisodeId}", episode.Id);

            return Ok(ApiResponse<PodcastEpisodeResponseDto>.SuccessResult(episode, "Episode başarıyla oluşturuldu"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreatePodcastEpisode error");
            return StatusCode(500, ApiResponse<PodcastEpisodeResponseDto>.ErrorResult("Bir hata oluştu"));
        }
    }
    
    /// <summary>
    /// Episode için audio dosyası upload et
    /// </summary>
    /// <param name="id">Episode ID</param>
    /// <param name="audioFile">MP3 audio dosyası</param>
    /// <returns>Upload sonucu</returns>
    /// <response code="200">Audio başarıyla upload edildi ve işleme alındı</response>
    /// <response code="400">Geçersiz dosya</response>
    /// <response code="401">Yetkisiz erişim</response>
    /// <response code="404">Episode bulunamadı</response>
    /// <response code="500">Sunucu hatası</response>
    /// <remarks>
    /// Bu endpoint daha önce oluşturulmuş bir episode için audio dosyası upload eder.
    /// 
    /// **Upload Süreci:**
    /// 1. Audio dosyası validation (MP3, max 500MB)
    /// 2. Dosya public/podcasts/{episodeId}/original.mp3 olarak kaydedilir
    /// 3. ProcessingStatus = Queued olarak güncellenir
    /// 4. RabbitMQ'ya processing mesajı gönderilir
    /// 5. Background service FFmpeg ile audio analysis yapar
    /// 6. Duration, bitrate ve quality versiyonları oluşturulur
    /// 
    /// **Audio Processing (Otomatik):**
    /// - Original kalite korunur
    /// - 256kbps, 128kbps, 64kbps versiyonları oluşturulur
    /// - Duration ve bitrate bilgisi çıkarılır
    /// </remarks>
    [HttpPost("episodes/{id}/upload-audio")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<PodcastEpisodeResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PodcastEpisodeResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<PodcastEpisodeResponseDto>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<PodcastEpisodeResponseDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<PodcastEpisodeResponseDto>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<PodcastEpisodeResponseDto>>> UploadEpisodeAudio(
        string id,
        [FromForm] IFormFile audioFile)
    {
        try
        {
            // Episode kontrolü
            var episode = await _podcastEpisodeService.GetByIdAsync(id);
            if (episode == null)
                return NotFound(ApiResponse<PodcastEpisodeResponseDto>.ErrorResult("Episode bulunamadı"));

            // Audio dosyası validation
            if (audioFile == null || audioFile.Length == 0)
                return BadRequest(ApiResponse<PodcastEpisodeResponseDto>.ErrorResult("Audio dosyası gerekli"));

            var isValidAudio = await _fileUploadService.ValidateAudioFileAsync(audioFile);
            if (!isValidAudio)
                return BadRequest(ApiResponse<PodcastEpisodeResponseDto>.ErrorResult("Geçersiz audio dosyası. Sadece MP3 formatı kabul edilir"));

            // Audio dosyası upload
            var audioFilePath = await _fileUploadService.UploadAudioFileAsync(
                audioFile, 
                episode.PodcastSeriesId, 
                episode.PodcastSeasonId, 
                episode.Id);

            // Episode'u güncelle - processing status'u Queued yap
            var updateDto = new UpdatePodcastEpisodeDto
            {
                OriginalAudioUrl = audioFilePath,
                OriginalFileName = audioFile.FileName,
                ProcessingStatus = Domain.Enums.ProcessingStatus.Queued
            };

            var updatedEpisode = await _podcastEpisodeService.UpdateAsync(episode.Id, updateDto);

            // RabbitMQ'ya processing mesajı gönder
            var processingMessage = new Infrastructure.Models.AudioProcessingMessage
            {
                EpisodeId = episode.Id,
                OriginalFilePath = audioFilePath,
                OriginalFileName = audioFile.FileName,
                QualityLevels = new List<Infrastructure.Models.AudioQualityRequest>
                {
                    new() { Quality = "64k", Bitrate = 64, OutputPath = $"podcasts/{episode.PodcastSeriesId}/{episode.PodcastSeasonId}/{episode.Id}/64k.mp3" },
                    new() { Quality = "128k", Bitrate = 128, OutputPath = $"podcasts/{episode.PodcastSeriesId}/{episode.PodcastSeasonId}/{episode.Id}/128k.mp3" },
                    new() { Quality = "256k", Bitrate = 256, OutputPath = $"podcasts/{episode.PodcastSeriesId}/{episode.PodcastSeasonId}/{episode.Id}/256k.mp3" }
                }
            };

            await _queueService.PublishAudioProcessingMessageAsync(processingMessage);

            _logger.LogInformation("Audio uploaded and queued for processing. EpisodeId: {EpisodeId}", episode.Id);

            return Ok(ApiResponse<PodcastEpisodeResponseDto>.SuccessResult(
                updatedEpisode ?? episode,
                "Audio dosyası başarıyla upload edildi ve işleme kuyruğa alındı"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UploadEpisodeAudio error with id: {id}", id);
            return StatusCode(500, ApiResponse<PodcastEpisodeResponseDto>.ErrorResult("Bir hata oluştu"));
        }
    }
    
    /// <summary>
    /// Episode için thumbnail upload et
    /// </summary>
    /// <param name="id">Episode ID</param>
    /// <param name="thumbnailFile">Thumbnail resmi (JPG, PNG, WEBP)</param>
    /// <returns>Upload sonucu</returns>
    /// <response code="200">Thumbnail başarıyla upload edildi</response>
    /// <response code="400">Geçersiz dosya</response>
    /// <response code="401">Yetkisiz erişim</response>
    /// <response code="404">Episode bulunamadı</response>
    /// <response code="500">Sunucu hatası</response>
    /// <remarks>
    /// Bu endpoint daha önce oluşturulmuş bir episode için thumbnail upload eder.
    /// 
    /// **Upload Klasörü:**
    /// - public/thumbnails/{seriesId}/{seasonId}/{episodeId}/thumbnail_{timestamp}.jpg
    /// 
    /// **Desteklenen Formatlar:**
    /// - JPG, JPEG, PNG, WEBP (maksimum 10MB)
    /// </remarks>
    [HttpPost("episodes/{id}/upload-thumbnail")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<PodcastEpisodeResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PodcastEpisodeResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<PodcastEpisodeResponseDto>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<PodcastEpisodeResponseDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<PodcastEpisodeResponseDto>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<PodcastEpisodeResponseDto>>> UploadEpisodeThumbnail(
        string id,
        [FromForm] IFormFile thumbnailFile)
    {
        try
        {
            // Episode kontrolü
            var episode = await _podcastEpisodeService.GetByIdAsync(id);
            if (episode == null)
                return NotFound(ApiResponse<PodcastEpisodeResponseDto>.ErrorResult("Episode bulunamadı"));

            // Thumbnail dosyası validation
            if (thumbnailFile == null || thumbnailFile.Length == 0)
                return BadRequest(ApiResponse<PodcastEpisodeResponseDto>.ErrorResult("Thumbnail dosyası gerekli"));

            var isValidThumbnail = await _fileUploadService.ValidateImageFileAsync(thumbnailFile);
            if (!isValidThumbnail)
                return BadRequest(ApiResponse<PodcastEpisodeResponseDto>.ErrorResult("Geçersiz thumbnail dosyası. JPG, PNG, WEBP formatları kabul edilir"));

            // Thumbnail upload
            var thumbnailUrl = await _fileUploadService.UploadThumbnailAsync(
                thumbnailFile,
                episode.PodcastSeriesId,
                episode.PodcastSeasonId,
                episode.Id);

            // Episode'u güncelle
            var updateDto = new UpdatePodcastEpisodeDto
            {
                ThumbnailUrl = thumbnailUrl
            };

            var updatedEpisode = await _podcastEpisodeService.UpdateAsync(episode.Id, updateDto);

            _logger.LogInformation("Thumbnail uploaded for episode. EpisodeId: {EpisodeId}", episode.Id);

            return Ok(ApiResponse<PodcastEpisodeResponseDto>.SuccessResult(
                updatedEpisode ?? episode,
                "Thumbnail başarıyla upload edildi"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UploadEpisodeThumbnail error with id: {id}", id);
            return StatusCode(500, ApiResponse<PodcastEpisodeResponseDto>.ErrorResult("Bir hata oluştu"));
        }
    }

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
