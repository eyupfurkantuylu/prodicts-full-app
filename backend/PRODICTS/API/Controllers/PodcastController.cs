using Application.Interface;
using Application.Models.DTOs;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

/// <summary>
/// Podcast yönetimi için public endpoint'ler
/// </summary>
/// <remarks>
/// Bu controller podcast içeriklerinin görüntülenmesi için genel erişim endpoint'lerini sağlar.
/// 
/// **Ana Özellikler:**
/// - Podcast serilerinin listelenmesi
/// - Podcast bölümlerinin görüntülenmesi  
/// - Quiz içeriklerinin alınması
/// - Son yayınlanan bölümlerin listelenmesi
/// 
/// **Desteklenen İşlemler:**
/// - Aktif podcast serilerini listeleme
/// - Seriye ait bölümleri görüntüleme
/// - Bölüm detaylarını alma
/// - Quiz sorularını listeleme
/// 
/// **Güvenlik:**
/// - Tüm endpoint'ler public erişime açık
/// - Sadece aktif içerikler döndürülür
/// - Yönetim işlemleri admin controller'da
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Tags("Podcast")]
[AllowAnonymous]
public class PodcastController : ControllerBase
{
    private readonly IPodcastSeriesService _podcastSeriesService;
    private readonly IPodcastSeasonService _podcastSeasonService;
    private readonly IPodcastEpisodeService _podcastEpisodeService;
    private readonly IPodcastQuizService _podcastQuizService;
    private readonly ILogger<PodcastController> _logger;

    public PodcastController(
        IPodcastSeriesService podcastSeriesService,
        IPodcastSeasonService podcastSeasonService,
        IPodcastEpisodeService podcastEpisodeService,
        IPodcastQuizService podcastQuizService,
        ILogger<PodcastController> logger)
    {
        _podcastSeriesService = podcastSeriesService;
        _podcastSeasonService = podcastSeasonService;
        _podcastEpisodeService = podcastEpisodeService;
        _podcastQuizService = podcastQuizService;
        _logger = logger;
    }

    /// <summary>
    /// Aktif podcast serilerini getir
    /// </summary>
    /// <returns>Aktif podcast serileri listesi</returns>
    /// <response code="200">Başarılı işlem - Aktif podcast serileri döndürüldü</response>
    /// <response code="500">Sunucu hatası - Beklenmeyen bir hata oluştu</response>
    /// <remarks>
    /// Bu endpoint tüm aktif podcast serilerini listeler. Sadece yayında olan seriler gösterilir.
    /// 
    /// **Kullanım Senaryoları:**
    /// - Ana sayfada podcast serilerini listelemek için
    /// - Kategori sayfalarında mevcut serileri göstermek için
    /// - Kullanıcının takip edebileceği serileri sunmak için
    /// 
    /// **Döndürülen Bilgiler:**
    /// - Seri ID, başlık, açıklama
    /// - Kapak görseli URL'i
    /// - Oluşturulma tarihi ve aktiflik durumu
    /// - Toplam bölüm sayısı
    /// 
    /// **Örnek Kullanım:**
    /// ```
    /// GET /api/podcast/series
    /// ```
    /// </remarks>
    [HttpGet("series")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PodcastSeriesResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PodcastSeriesResponseDto>>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<IEnumerable<PodcastSeriesResponseDto>>>> GetActiveSeries()
    {
        try
        {
            var series = await _podcastSeriesService.GetAllActiveAsync();
            return Ok(ApiResponse<IEnumerable<PodcastSeriesResponseDto>>.SuccessResult(series));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetActiveSeries error");
            return StatusCode(500, ApiResponse<IEnumerable<PodcastSeriesResponseDto>>.ErrorResult("Bir hata oluştu"));
        }
    }

    /// <summary>
    /// Podcast serisi detayını getir
    /// </summary>
    /// <param name="id">Podcast serisi ID'si (MongoDB ObjectId formatında)</param>
    /// <returns>Podcast serisi detay bilgileri</returns>
    /// <response code="200">Başarılı işlem - Seri detayları döndürüldü</response>
    /// <response code="404">Belirtilen ID'ye sahip podcast serisi bulunamadı</response>
    /// <response code="500">Sunucu hatası - Beklenmeyen bir hata oluştu</response>
    /// <remarks>
    /// Belirli bir podcast serisinin detay bilgilerini getirir.
    /// 
    /// **Kullanım Senaryoları:**
    /// - Seri detay sayfasını göstermek için
    /// - Sezon listesi göstermeden önce seri bilgilerini almak için
    /// - Meta data bilgilerini (açıklama, kapak, vb.) göstermek için
    /// 
    /// **Döndürülen Bilgiler:**
    /// - Seri ID, başlık, detaylı açıklama
    /// - Kapak görseli ve thumbnail URL'leri
    /// - Oluşturulma tarihi ve son güncelleme
    /// - Aktiflik durumu ve görünürlük ayarları
    /// 
    /// **Özel Durumlar:**
    /// - Geçersiz ObjectId formatı durumunda 400 hatası
    /// - Seri mevcut değilse 404 hatası döner
    /// 
    /// **Örnek Kullanım:**
    /// ```
    /// GET /api/podcast/series/507f1f77bcf86cd799439011
    /// ```
    /// </remarks>
    [HttpGet("series/{id}")]
    [ProducesResponseType(typeof(ApiResponse<PodcastSeriesResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PodcastSeriesResponseDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<PodcastSeriesResponseDto>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<PodcastSeriesResponseDto>>> GetSeriesById(string id)
    {
        try
        {
            var series = await _podcastSeriesService.GetByIdAsync(id);
            if (series == null)
                return NotFound(ApiResponse<PodcastSeriesResponseDto>.ErrorResult("Podcast serisi bulunamadı"));

            return Ok(ApiResponse<PodcastSeriesResponseDto>.SuccessResult(series));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetSeriesById error with id: {id}", id);
            return StatusCode(500, ApiResponse<PodcastSeriesResponseDto>.ErrorResult("Bir hata oluştu"));
        }
    }

    /// <summary>
    /// Seriye ait sezonları getir
    /// </summary>
    /// <param name="seriesId">Podcast serisi ID'si (MongoDB ObjectId formatında)</param>
    /// <returns>Belirtilen seriye ait sezon listesi</returns>
    /// <response code="200">Başarılı işlem - Sezon listesi döndürüldü</response>
    /// <response code="500">Sunucu hatası - Beklenmeyen bir hata oluştu</response>
    /// <remarks>
    /// Belirli bir podcast serisine ait tüm sezonları listeler.
    /// 
    /// **Kullanım Senaryoları:**
    /// - Seri detay sayfasında sezonları listelemek için
    /// - Kullanıcının hangi sezonları dinleyebileceğini göstermek için
    /// - Sezon seçimi için dropdown/liste oluşturmak için
    /// 
    /// **Döndürülen Bilgiler:**
    /// - Sezon ID, numara, başlık ve açıklama
    /// - Sezona ait bölüm sayısı
    /// - Sezonun başlangıç ve bitiş tarihleri
    /// - Aktiflik durumu
    /// 
    /// **Özel Durumlar:**
    /// - Seri mevcut değilse boş liste döner
    /// - Sadece aktif sezonlar listelenir
    /// - Sezonlar numaraya göre sıralı döner
    /// 
    /// **Örnek Kullanım:**
    /// ```
    /// GET /api/podcast/series/507f1f77bcf86cd799439011/seasons
    /// ```
    /// </remarks>
    [HttpGet("series/{seriesId}/seasons")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PodcastSeasonResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PodcastSeasonResponseDto>>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<IEnumerable<PodcastSeasonResponseDto>>>> GetSeasonsBySeriesId(string seriesId)
    {
        try
        {
            var seasons = await _podcastSeasonService.GetBySeriesIdAsync(seriesId);
            return Ok(ApiResponse<IEnumerable<PodcastSeasonResponseDto>>.SuccessResult(seasons));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetSeasonsBySeriesId error with seriesId: {seriesId}", seriesId);
            return StatusCode(500, ApiResponse<IEnumerable<PodcastSeasonResponseDto>>.ErrorResult("Bir hata oluştu"));
        }
    }

    /// <summary>
    /// Sezon detayını getir
    /// </summary>
    /// <param name="id">Podcast sezonu ID'si (MongoDB ObjectId formatında)</param>
    /// <returns>Podcast sezonu detay bilgileri</returns>
    /// <response code="200">Başarılı işlem - Sezon detayları döndürüldü</response>
    /// <response code="404">Belirtilen ID'ye sahip podcast sezonu bulunamadı</response>
    /// <response code="500">Sunucu hatası - Beklenmeyen bir hata oluştu</response>
    /// <remarks>
    /// Belirli bir podcast sezonunun detay bilgilerini getirir.
    /// 
    /// **Kullanım Senaryoları:**
    /// - Sezon detay sayfasını göstermek için
    /// - Sezon bölümlerini listelemeden önce sezon bilgilerini almak için
    /// - Sezon meta verilerini göstermek için
    /// 
    /// **Döndürülen Bilgiler:**
    /// - Sezon ID, numara, başlık ve açıklama
    /// - Ait olduğu seri bilgileri
    /// - Sezon kapak görseli ve thumbnail
    /// - Aktiflik durumu ve yayın tarihleri
    /// 
    /// **Örnek Kullanım:**
    /// ```
    /// GET /api/podcast/seasons/507f1f77bcf86cd799439012
    /// ```
    /// </remarks>
    [HttpGet("seasons/{id}")]
    [ProducesResponseType(typeof(ApiResponse<PodcastSeasonResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PodcastSeasonResponseDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<PodcastSeasonResponseDto>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<PodcastSeasonResponseDto>>> GetSeasonById(string id)
    {
        try
        {
            var season = await _podcastSeasonService.GetByIdAsync(id);
            if (season == null)
                return NotFound(ApiResponse<PodcastSeasonResponseDto>.ErrorResult("Podcast sezonu bulunamadı"));

            return Ok(ApiResponse<PodcastSeasonResponseDto>.SuccessResult(season));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetSeasonById error with id: {id}", id);
            return StatusCode(500, ApiResponse<PodcastSeasonResponseDto>.ErrorResult("Bir hata oluştu"));
        }
    }

    /// <summary>
    /// Seriye ait bölümleri getir
    /// </summary>
    /// <param name="seriesId">Podcast serisi ID'si (MongoDB ObjectId formatında)</param>
    /// <returns>Belirtilen seriye ait tüm aktif bölümler</returns>
    /// <response code="200">Başarılı işlem - Bölüm listesi döndürüldü</response>
    /// <response code="500">Sunucu hatası - Beklenmeyen bir hata oluştu</response>
    /// <remarks>
    /// Belirli bir podcast serisine ait tüm aktif bölümleri listeler. Tüm sezonlardan bölümler dahil edilir.
    /// 
    /// **Kullanım Senaryoları:**
    /// - Seri sayfasında tüm bölümleri kronolojik sırada göstermek için
    /// - "Tüm Bölümler" sekmesi için
    /// - Arama sonuçlarında seri bazlı filtreleme için
    /// 
    /// **Döndürülen Bilgiler:**
    /// - Bölüm ID, numara, başlık ve açıklama
    /// - Audio URL ve ses kalite seçenekleri
    /// - Yayın tarihi ve süre bilgisi
    /// - Ait olduğu seri ve sezon bilgileri (SeriesTitle, SeasonTitle, vb.)
    /// - Thumbnail URL'i
    /// 
    /// **Özel Durumlar:**
    /// - Sadece aktif (yayında) bölümler döner
    /// - Bölümler yayın tarihine göre sıralı döner
    /// - Her bölümde series ve season detayları da yer alır
    /// 
    /// **Örnek Kullanım:**
    /// ```
    /// GET /api/podcast/series/507f1f77bcf86cd799439011/episodes
    /// ```
    /// </remarks>
    [HttpGet("series/{seriesId}/episodes")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PodcastEpisodeResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PodcastEpisodeResponseDto>>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<IEnumerable<PodcastEpisodeResponseDto>>>> GetEpisodesBySeriesId(string seriesId)
    {
        try
        {
            var episodes = await _podcastEpisodeService.GetBySeriesIdAsync(seriesId);
            return Ok(ApiResponse<IEnumerable<PodcastEpisodeResponseDto>>.SuccessResult(episodes));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetEpisodesBySeriesId error with seriesId: {seriesId}", seriesId);
            return StatusCode(500, ApiResponse<IEnumerable<PodcastEpisodeResponseDto>>.ErrorResult("Bir hata oluştu"));
        }
    }

    /// <summary>
    /// Sezona ait bölümleri getir
    /// </summary>
    /// <param name="seasonId">Podcast sezonu ID'si (MongoDB ObjectId formatında)</param>
    /// <returns>Belirtilen sezona ait tüm aktif bölümler</returns>
    /// <response code="200">Başarılı işlem - Sezon bölümleri döndürüldü</response>
    /// <response code="500">Sunucu hatası - Beklenmeyen bir hata oluştu</response>
    /// <remarks>
    /// Belirli bir podcast sezonuna ait tüm aktif bölümleri listeler. Sadece o sezonun bölümleri dahil edilir.
    /// 
    /// **Kullanım Senaryoları:**
    /// - Sezon detay sayfasında o sezona ait bölümleri göstermek için
    /// - Kullanıcı belirli bir sezonu seçtiğinde bölümleri listelemek için
    /// - "Bu Sezondaki Bölümler" sekmesi için
    /// - Sezon bazlı çalma listesi oluşturmak için
    /// 
    /// **Döndürülen Bilgiler:**
    /// - Bölüm ID, numara, başlık ve açıklama
    /// - Audio URL ve ses kalite seçenekleri
    /// - Yayın tarihi ve süre bilgisi
    /// - Ait olduğu seri ve sezon bilgileri (SeriesTitle, SeasonTitle, vb.)
    /// - Thumbnail URL'i
    /// 
    /// **Avantajları:**
    /// - Daha odaklı içerik listeleme
    /// - Sezon bazlı gezinme imkanı
    /// - Belirli konulara odaklanmış dinleme deneyimi
    /// 
    /// **Özel Durumlar:**
    /// - Sadece aktif (yayında) bölümler döner
    /// - Bölümler episode numarasına göre sıralı döner
    /// - Her bölümde series ve season detayları da yer alır
    /// 
    /// **Örnek Kullanım:**
    /// ```
    /// GET /api/podcast/seasons/507f1f77bcf86cd799439012/episodes
    /// ```
    /// </remarks>
    [HttpGet("seasons/{seasonId}/episodes")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PodcastEpisodeResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PodcastEpisodeResponseDto>>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<IEnumerable<PodcastEpisodeResponseDto>>>> GetEpisodesBySeasonId(string seasonId)
    {
        try
        {
            var episodes = await _podcastEpisodeService.GetBySeasonIdAsync(seasonId);
            return Ok(ApiResponse<IEnumerable<PodcastEpisodeResponseDto>>.SuccessResult(episodes));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetEpisodesBySeasonId error with seasonId: {seasonId}", seasonId);
            return StatusCode(500, ApiResponse<IEnumerable<PodcastEpisodeResponseDto>>.ErrorResult("Bir hata oluştu"));
        }
    }

    /// <summary>
    /// Bölüm detayını getir
    /// </summary>
    /// <param name="id">Podcast bölümü ID'si (MongoDB ObjectId formatında)</param>
    /// <returns>Podcast bölümü detay bilgileri</returns>
    /// <response code="200">Başarılı işlem - Bölüm detayları döndürüldü</response>
    /// <response code="404">Belirtilen ID'ye sahip podcast bölümü bulunamadı</response>
    /// <response code="500">Sunucu hatası - Beklenmeyen bir hata oluştu</response>
    /// <remarks>
    /// Belirli bir podcast bölümünün tüm detay bilgilerini getirir. Bu endpoint oynatıcı için gerekli tüm bilgileri sağlar.
    /// 
    /// **Kullanım Senaryoları:**
    /// - Bölüm oynatıcı sayfasını göstermek için
    /// - Audio player'da gösterilecek meta verileri almak için
    /// - Bölüm paylaşım bilgilerini oluşturmak için
    /// - İlgili quiz ve içerikleri göstermek için hazırlık
    /// 
    /// **Döndürülen Bilgiler:**
    /// - Tam bölüm bilgileri (ID, başlık, açıklama, süre)
    /// - Audio dosya URL'i ve kalite seçenekleri
    /// - Yayın tarihi ve oluşturulma zamanı
    /// - Bağlı olduğu seri bilgileri (SeriesTitle, SeriesDescription)
    /// - Bağlı olduğu sezon bilgileri (SeasonTitle, SeasonNumber)
    /// - Thumbnail ve kapak görselleri
    /// 
    /// **Player Integration:**
    /// - Audio URL direkt oynatıcıda kullanılabilir
    /// - AudioQualities JSON'dan farklı kalite seçenekleri parse edilebilir
    /// - Meta veriler player interface'inde gösterilebilir
    /// 
    /// **Örnek Kullanım:**
    /// ```
    /// GET /api/podcast/episodes/507f1f77bcf86cd799439013
    /// ```
    /// </remarks>
    [HttpGet("episodes/{id}")]
    [ProducesResponseType(typeof(ApiResponse<PodcastEpisodeResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PodcastEpisodeResponseDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<PodcastEpisodeResponseDto>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<PodcastEpisodeResponseDto>>> GetEpisodeById(string id)
    {
        try
        {
            var episode = await _podcastEpisodeService.GetByIdAsync(id);
            if (episode == null)
                return NotFound(ApiResponse<PodcastEpisodeResponseDto>.ErrorResult("Podcast bölümü bulunamadı"));

            return Ok(ApiResponse<PodcastEpisodeResponseDto>.SuccessResult(episode));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetEpisodeById error with id: {id}", id);
            return StatusCode(500, ApiResponse<PodcastEpisodeResponseDto>.ErrorResult("Bir hata oluştu"));
        }
    }

    /// <summary>
    /// Son yayınlanan bölümleri getir
    /// </summary>
    /// <param name="count">Getirilecek bölüm sayısı (varsayılan: 10, maksimum: 50)</param>
    /// <returns>En son yayınlanan podcast bölümleri</returns>
    /// <response code="200">Başarılı işlem - Son bölümler döndürüldü</response>
    /// <response code="500">Sunucu hatası - Beklenmeyen bir hata oluştu</response>
    /// <remarks>
    /// Tüm serilerden en son yayınlanan podcast bölümlerini yayın tarihine göre sıralı olarak getirir.
    /// 
    /// **Kullanım Senaryoları:**
    /// - Ana sayfa "Yeni Bölümler" bölümü için
    /// - "Son Çıkanlar" widget'ı için
    /// - RSS feed benzeri güncel içerik akışı için
    /// - Kullanıcılara yeni içerik önerileri sunmak için
    /// 
    /// **Döndürülen Bilgiler:**
    /// - En güncel bölümler (yayın tarihine göre azalan sıra)
    /// - Tam bölüm bilgileri ve meta veriler
    /// - Ait olduğu seri ve sezon bilgileri
    /// - Oynatıcı için gerekli audio URL'leri
    /// - Thumbnail ve görseller
    /// 
    /// **Filtreleme:**
    /// - Sadece aktif (yayında) bölümler dahil edilir
    /// - Gelecek tarihli bölümler dahil edilmez
    /// - Tüm serilerden karışık olarak seçilir
    /// 
    /// **Performans:**
    /// - Maksimum 50 bölüm getirilebilir
    /// - Veritabanında index'li sorgulama
    /// - Yayın tarihine göre optimize edilmiş sıralama
    /// 
    /// **Örnek Kullanım:**
    /// ```
    /// GET /api/podcast/episodes/latest
    /// GET /api/podcast/episodes/latest?count=20
    /// ```
    /// </remarks>
    [HttpGet("episodes/latest")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PodcastEpisodeResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PodcastEpisodeResponseDto>>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<IEnumerable<PodcastEpisodeResponseDto>>>> GetLatestEpisodes([FromQuery] int count = 10)
    {
        try
        {
            var episodes = await _podcastEpisodeService.GetLatestEpisodesAsync(count);
            return Ok(ApiResponse<IEnumerable<PodcastEpisodeResponseDto>>.SuccessResult(episodes));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetLatestEpisodes error with count: {count}", count);
            return StatusCode(500, ApiResponse<IEnumerable<PodcastEpisodeResponseDto>>.ErrorResult("Bir hata oluştu"));
        }
    }

    /// <summary>
    /// Bölüme ait quizleri getir
    /// </summary>
    /// <param name="episodeId">Podcast bölümü ID'si (MongoDB ObjectId formatında)</param>
    /// <returns>Belirtilen bölüme ait quiz soruları</returns>
    /// <response code="200">Başarılı işlem - Quiz listesi döndürüldü</response>
    /// <response code="500">Sunucu hatası - Beklenmeyen bir hata oluştu</response>
    /// <remarks>
    /// Belirli bir podcast bölümüne ait tüm quiz sorularını getirir. Bu quizler bölüm dinlendikten sonra öğrenme takviyesi için kullanılır.
    /// 
    /// **Kullanım Senaryoları:**
    /// - Podcast bölümü bittikten sonra quiz göstermek için
    /// - Eğitim amaçlı içerik takviyesi için
    /// - Kullanıcının öğrenme durumunu test etmek için
    /// - Gamification ve etkileşim artırma için
    /// 
    /// **Döndürülen Bilgiler:**
    /// - Quiz ID, soru metni ve seçenekler
    /// - Doğru cevap bilgisi
    /// - Zorluk seviyesi ve puan değeri
    /// - Quiz türü (çoktan seçmeli, doğru/yanlış, vb.)
    /// - Açıklama ve öğretici notlar
    /// 
    /// **Quiz Türleri:**
    /// - Kelime anlamı soruları
    /// - Dinleme anlama soruları
    /// - Telaffuz değerlendirme soruları
    /// - Gramer ve yapı soruları
    /// 
    /// **Entegrasyon:**
    /// - Bölüm player'ında quiz butonu için
    /// - Öğrenme progress tracking için
    /// - Kullanıcı skorlama sistemi için
    /// 
    /// **Örnek Kullanım:**
    /// ```
    /// GET /api/podcast/episodes/507f1f77bcf86cd799439013/quizzes
    /// ```
    /// </remarks>
    [HttpGet("episodes/{episodeId}/quizzes")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PodcastQuizResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PodcastQuizResponseDto>>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<IEnumerable<PodcastQuizResponseDto>>>> GetQuizzesByEpisodeId(string episodeId)
    {
        try
        {
            var quizzes = await _podcastQuizService.GetByEpisodeIdAsync(episodeId);
            return Ok(ApiResponse<IEnumerable<PodcastQuizResponseDto>>.SuccessResult(quizzes));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetQuizzesByEpisodeId error with episodeId: {episodeId}", episodeId);
            return StatusCode(500, ApiResponse<IEnumerable<PodcastQuizResponseDto>>.ErrorResult("Bir hata oluştu"));
        }
    }

    /// <summary>
    /// Quiz detayını getir
    /// </summary>
    /// <param name="id">Quiz ID'si (MongoDB ObjectId formatında)</param>
    /// <returns>Quiz detay bilgileri</returns>
    /// <response code="200">Başarılı işlem - Quiz detayları döndürüldü</response>
    /// <response code="404">Belirtilen ID'ye sahip quiz bulunamadı</response>
    /// <response code="500">Sunucu hatası - Beklenmeyen bir hata oluştu</response>
    /// <remarks>
    /// Belirli bir quiz sorusunun tüm detay bilgilerini getirir. Bu endpoint quiz çözüm ekranı için kullanılır.
    /// 
    /// **Kullanım Senaryoları:**
    /// - Quiz çözüm sayfasını göstermek için
    /// - Tek soru detayını almak için
    /// - Cevap kontrolü yapmak için
    /// - Quiz sonuç ekranı için detay bilgiler
    /// 
    /// **Döndürülen Bilgiler:**
    /// - Tam soru metni ve seçenekler
    /// - Doğru cevap ve açıklama
    /// - Zorluk seviyesi ve puan değeri
    /// - Hangi podcast bölümüne ait olduğu
    /// - Quiz türü ve meta veriler
    /// 
    /// **Güvenlik:**
    /// - Doğru cevap bilgisi sadece cevap verildikten sonra gösterilmeli
    /// - Client-side validation için değil, server-side kontrol için kullanılmalı
    /// 
    /// **Quiz Çözüm Akışı:**
    /// 1. Bu endpoint ile quiz detayları alınır
    /// 2. Kullanıcı cevap verir
    /// 3. Cevap server'a gönderilir ve kontrol edilir
    /// 4. Sonuç ve açıklama gösterilir
    /// 
    /// **Örnek Kullanım:**
    /// ```
    /// GET /api/podcast/quizzes/507f1f77bcf86cd799439014
    /// ```
    /// </remarks>
    [HttpGet("quizzes/{id}")]
    [ProducesResponseType(typeof(ApiResponse<PodcastQuizResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PodcastQuizResponseDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<PodcastQuizResponseDto>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<PodcastQuizResponseDto>>> GetQuizById(string id)
    {
        try
        {
            var quiz = await _podcastQuizService.GetByIdAsync(id);
            if (quiz == null)
                return NotFound(ApiResponse<PodcastQuizResponseDto>.ErrorResult("Quiz bulunamadı"));

            return Ok(ApiResponse<PodcastQuizResponseDto>.SuccessResult(quiz));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetQuizById error with id: {id}", id);
            return StatusCode(500, ApiResponse<PodcastQuizResponseDto>.ErrorResult("Bir hata oluştu"));
        }
    }
}
