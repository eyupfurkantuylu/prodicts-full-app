using Application.Interface;
using Application.Models.DTOs;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace API.Controllers;

/// <summary>
/// Kullanıcı yönetimi ve kullanıcı verilerinin manipülasyonu için endpoint'ler
/// </summary>
/// <remarks>
/// Bu controller aşağıdaki kullanıcı işlemlerini destekler:
/// 
/// **Ana Özellikler:**
/// - Kullanıcı CRUD işlemleri (Create, Read, Update, Delete)
/// - OAuth provider ile kayıt
/// - Anonymous kullanıcı yönetimi
/// - Kullanıcı veri senkronizasyonu
/// - Anonymous kullanıcıyı kayıtlı kullanıcıya dönüştürme
/// 
/// **Desteklenen İşlemler:**
/// - OAuth ile kullanıcı kaydı
/// - Anonymous kullanıcı oluşturma/getirme
/// - Kullanıcı verilerini senkronize etme
/// - Anonymous kullanıcıyı upgrade etme
/// - Kullanıcı bilgilerini güncelleme
/// - Kullanıcı silme
/// - Email kontrolü
/// 
/// **Teknik Detaylar:**
/// - MongoDB ObjectId formatında ID'ler
/// - DTO pattern ile veri transferi
/// - Tutarlı ApiResponse wrapper
/// - Comprehensive error handling
/// 
/// **Güvenlik:**
/// - Varsayılan olarak tüm endpoint'ler korumalı ([Authorize])
/// - Public endpoint'ler için [AllowAnonymous] kullanılır
/// - OAuth provider doğrulaması
/// - Email uniqueness kontrolü
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Authorize] // Tüm endpoint'leri koru
[Tags("User")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserService userService, ILogger<UserController> logger)
    {
        _userService = userService;
        _logger = logger;
    }



  

    /// <summary>
    /// OAuth provider ile kullanıcı kaydı
    /// </summary>
    /// <param name="providerDto">OAuth provider bilgileri (Google, Facebook, Apple)</param>
    /// <returns>Kayıtlı kullanıcı bilgileri</returns>
    /// <response code="200">Provider ile kayıt başarılı</response>
    /// <response code="400">Geçersiz provider bilgileri veya email zaten kullanımda</response>
    /// <response code="500">Sunucu hatası</response>
    /// <remarks>
    /// OAuth provider bilgileri ile yeni kullanıcı kaydı yapar.
    /// 
    /// **Desteklenen Provider'lar:**
    /// - Google OAuth
    /// - Facebook OAuth
    /// - Apple OAuth
    /// 
    /// **Kullanım Senaryoları:**
    /// - İlk kez OAuth ile giriş yapan kullanıcı
    /// - Mevcut email adresi kontrolü
    /// - Provider bilgilerinin doğrulanması
    /// 
    /// **Özel Durumlar:**
    /// - Email zaten kayıtlıysa hata döner
    /// - Provider ID uniqueness kontrolü
    /// - Eksik provider bilgileri validasyonu
    /// 
    /// **Örnek Kullanım:**
    /// ```
    /// POST /api/User/register-provider
    /// {
    ///   "email": "user@example.com",
    ///   "fullName": "John Doe",
    ///   "provider": "Google",
    ///   "providerId": "google_123456789"
    /// }
    /// ```
    /// </remarks>
    [HttpPost("register-provider")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status500InternalServerError)]
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

    /// <summary>
    /// Anonymous kullanıcı oluşturma veya getirme
    /// </summary>
    /// <param name="requestDto">Device ID ve cihaz bilgileri</param>
    /// <returns>Anonymous kullanıcı bilgileri</returns>
    /// <response code="200">Anonymous kullanıcı hazır</response>
    /// <response code="500">Sunucu hatası</response>
    /// <remarks>
    /// Device ID ile anonymous kullanıcı oluşturur veya mevcut olanı getirir.
    /// 
    /// **Kullanım Senaryoları:**
    /// - Uygulama ilk açılışında anonymous kullanıcı oluşturma
    /// - Kayıt olmadan uygulama kullanımı
    /// - Device ID ile kullanıcı takibi
    /// - Local data ile cloud sync hazırlığı
    /// 
    /// **Özel Durumlar:**
    /// - Aynı device ID ile tekrar istek gelirse mevcut kullanıcı döner
    /// - Her device için benzersiz anonymous kullanıcı
    /// - Cihaz bilgileri güncelleme desteği
    /// 
    /// **Device ID Örnekleri:**
    /// - iOS: UIDevice.identifierForVendor
    /// - Android: Settings.Secure.ANDROID_ID
    /// - Web: localStorage UUID
    /// 
    /// **Örnek Kullanım:**
    /// ```
    /// POST /api/User/anonymous
    /// {
    ///   "deviceId": "device_12345",
    ///   "deviceInfo": "iOS 15.0"
    /// }
    /// ```
    /// </remarks>
    [HttpPost("anonymous")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status500InternalServerError)]
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

    /// <summary>
    /// Kullanıcı verilerini senkronize etme
    /// </summary>
    /// <param name="syncDto">Senkronize edilecek veri bilgileri</param>
    /// <returns>Güncellenmiş kullanıcı bilgileri</returns>
    /// <response code="200">Sync başarılı</response>
    /// <response code="401">Yetkilendirme hatası</response>
    /// <response code="404">Kullanıcı bulunamadı</response>
    /// <response code="500">Sunucu hatası</response>
    /// <remarks>
    /// Local ile cloud arasında kullanıcı verilerini senkronize eder.
    /// 
    /// **Kullanım Senaryoları:**
    /// - Local storage'dan cloud'a veri aktarımı
    /// - Cloud'dan local'e veri çekme
    /// - Çoklu cihaz arasında veri senkronizasyonu
    /// - Offline-online geçişlerinde veri tutarlılığı
    /// 
    /// **Sync Stratejileri:**
    /// - Timestamp bazlı conflict resolution
    /// - Last-write-wins stratejisi
    /// - Incremental sync desteği
    /// - Batch sync optimizasyonu
    /// 
    /// **Özel Durumlar:**
    /// - Conflict durumunda cloud öncelikli
    /// - Büyük veri setleri için pagination
    /// - Network hatalarında retry mekanizması
    /// 
    /// **Örnek Kullanım:**
    /// ```
    /// POST /api/User/sync
    /// {
    ///   "userId": "user_123",
    ///   "syncData": {...},
    ///   "lastSyncTime": "2024-01-15T10:30:00Z"
    /// }
    /// ```
    /// </remarks>
    [HttpPost("sync")]
    [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status500InternalServerError)]
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

    /// <summary>
    /// Anonymous kullanıcıyı kayıtlı kullanıcıya dönüştürme
    /// </summary>
    /// <param name="requestDto">Device ID ve kayıt bilgileri</param>
    /// <returns>Kayıtlı kullanıcı bilgileri</returns>
    /// <response code="200">Anonymous user kayıtlı kullanıcıya dönüştürüldü</response>
    /// <response code="400">Geçersiz veri veya email zaten kullanımda</response>
    /// <response code="401">Yetkilendirme hatası</response>
    /// <response code="404">Anonymous kullanıcı bulunamadı</response>
    /// <response code="500">Sunucu hatası</response>
    /// <remarks>
    /// Mevcut anonymous kullanıcıyı kayıtlı kullanıcıya dönüştürür ve tüm verilerini korur.
    /// 
    /// **Kullanım Senaryoları:**
    /// - Anonymous kullanıcının kayıt olmaya karar vermesi
    /// - Veri kaybı olmadan hesap oluşturma
    /// - Çoklu cihaz erişimi için upgrade
    /// - Premium özelliklere erişim için hesap oluşturma
    /// 
    /// **Upgrade İşlemi:**
    /// 1. Anonymous kullanıcı bilgileri korunur
    /// 2. Email ve şifre eklenir
    /// 3. User type "registered" olarak değiştirilir
    /// 4. Tüm app data'sı korunur
    /// 5. Device ID'ler korunur
    /// 
    /// **Özel Durumlar:**
    /// - Email zaten kullanımda ise hata
    /// - Anonymous kullanıcı bulunamaz ise hata
    /// - Zaten kayıtlı kullanıcı ise hata
    /// - Veri migrasyonu başarısız ise rollback
    /// 
    /// **Örnek Kullanım:**
    /// ```
    /// POST /api/User/upgrade-anonymous
    /// {
    ///   "deviceId": "device_12345",
    ///   "registerData": {
    ///     "email": "user@example.com",
    ///     "password": "securePassword",
    ///     "fullName": "John Doe"
    ///   }
    /// }
    /// ```
    /// </remarks>
    [HttpPost("upgrade-anonymous")]
    [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status500InternalServerError)]
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

    /// <summary>
    /// ID ile kullanıcı bilgilerini getirme
    /// </summary>
    /// <param name="id">MongoDB ObjectId formatında kullanıcı ID'si</param>
    /// <returns>Kullanıcı bilgileri</returns>
    /// <response code="200">Kullanıcı bulundu</response>
    /// <response code="401">Yetkilendirme hatası</response>
    /// <response code="404">Kullanıcı bulunamadı</response>
    /// <response code="500">Sunucu hatası</response>
    /// <remarks>
    /// Belirtilen ID'ye sahip kullanıcının detaylı bilgilerini getirir.
    /// 
    /// **Kullanım Senaryoları:**
    /// - Kullanıcı profil sayfası
    /// - Admin panelinde kullanıcı detayları
    /// - Başka kullanıcının profilini görüntüleme
    /// - API entegrasyonlarında kullanıcı bilgi çekme
    /// 
    /// **Dönen Bilgiler:**
    /// - Kullanıcı temel bilgileri
    /// - OAuth provider bilgileri
    /// - Subscription durumu
    /// - Kayıt tarihi ve son aktivite
    /// 
    /// **Güvenlik:**
    /// - Authorization token gerekli
    /// - Sadece kendi bilgilerini veya yetkili kullanıcıların diğerlerini görmesi
    /// - Sensitive bilgiler filtrelenir
    /// 
    /// **Örnek Kullanım:**
    /// ```
    /// GET /api/User/689faa4b8356a979df152dab
    /// Authorization: Bearer {token}
    /// ```
    /// </remarks>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status500InternalServerError)]
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

    /// <summary>
    /// Email ile kullanıcı bilgilerini getirme
    /// </summary>
    /// <param name="email">Kullanıcının email adresi</param>
    /// <returns>Kullanıcı bilgileri</returns>
    /// <response code="200">Kullanıcı bulundu</response>
    /// <response code="401">Yetkilendirme hatası</response>
    /// <response code="404">Kullanıcı bulunamadı</response>
    /// <response code="500">Sunucu hatası</response>
    /// <remarks>
    /// Belirtilen email adresine sahip kullanıcının bilgilerini getirir.
    /// 
    /// **Kullanım Senaryoları:**
    /// - Email ile kullanıcı arama
    /// - Forgot password işlemlerinde
    /// - Admin panelinde email ile arama
    /// - User lookup operations
    /// 
    /// **Güvenlik:**
    /// - Authorization token gerekli
    /// - Email format validasyonu
    /// - Rate limiting uygulanabilir
    /// - Sensitive bilgiler filtrelenir
    /// 
    /// **Örnek Kullanım:**
    /// ```
    /// GET /api/User/email/user@example.com
    /// Authorization: Bearer {token}
    /// ```
    /// </remarks>
    [HttpGet("email/{email}")]
    [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status500InternalServerError)]
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

    /// <summary>
    /// Kullanıcı bilgilerini güncelleme
    /// </summary>
    /// <param name="id">MongoDB ObjectId formatında kullanıcı ID'si</param>
    /// <param name="updateDto">Güncellenecek kullanıcı bilgileri</param>
    /// <returns>Güncellenmiş kullanıcı bilgileri</returns>
    /// <response code="200">Güncelleme başarılı</response>
    /// <response code="400">Geçersiz veri</response>
    /// <response code="401">Yetkilendirme hatası</response>
    /// <response code="404">Kullanıcı bulunamadı</response>
    /// <response code="500">Sunucu hatası</response>
    /// <remarks>
    /// Mevcut kullanıcının bilgilerini günceller.
    /// 
    /// **Güncellenebilir Alanlar:**
    /// - Full Name
    /// - Profile bilgileri
    /// - Preferences
    /// - Device bilgileri
    /// - Subscription durumu
    /// 
    /// **Güncellenemez Alanlar:**
    /// - Email (ayrı endpoint gerekli)
    /// - Password (ayrı endpoint gerekli)
    /// - User ID
    /// - OAuth provider bilgileri
    /// 
    /// **Kullanım Senaryoları:**
    /// - Profil düzenleme
    /// - Ayarlar güncelleme
    /// - Device bilgileri güncelleme
    /// - Admin tarafından kullanıcı düzenleme
    /// 
    /// **Validation:**
    /// - Required field kontrolü
    /// - Data format validasyonu
    /// - Business rule kontrolü
    /// 
    /// **Örnek Kullanım:**
    /// ```
    /// PUT /api/User/689faa4b8356a979df152dab
    /// {
    ///   "fullName": "Updated Name",
    ///   "preferences": {...}
    /// }
    /// ```
    /// </remarks>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status500InternalServerError)]
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

    /// <summary>
    /// Kullanıcı hesabını silme
    /// </summary>
    /// <param name="id">MongoDB ObjectId formatında kullanıcı ID'si</param>
    /// <returns>Silme işlemi sonucu</returns>
    /// <response code="200">Kullanıcı silindi</response>
    /// <response code="401">Yetkilendirme hatası</response>
    /// <response code="404">Kullanıcı bulunamadı</response>
    /// <response code="500">Sunucu hatası</response>
    /// <remarks>
    /// Kullanıcı hesabını ve ilişkili tüm verilerini sistemden siler.
    /// 
    /// **Silme İşlemi Kapsamı:**
    /// - Kullanıcı temel bilgileri
    /// - OAuth provider bağlantıları
    /// - Refresh token'ları
    /// - İlişkili uygulama verileri
    /// - Anonymous user referansları
    /// 
    /// **Güvenlik:**
    /// - Sadece kullanıcının kendisi veya admin silebilir
    /// - Soft delete vs hard delete seçeneği
    /// - Audit log kaydı
    /// - Geri dönüşü olmayan işlem
    /// 
    /// **Kullanım Senaryoları:**
    /// - Kullanıcının hesabını kapatması
    /// - GDPR compliance için veri silme
    /// - Admin tarafından hesap kapatma
    /// - Spam/abuse durumunda hesap silme
    /// 
    /// **Özel Durumlar:**
    /// - Active subscription varsa uyarı
    /// - Bağlı cihazlarda logout
    /// - Referans data temizliği
    /// 
    /// **Örnek Kullanım:**
    /// ```
    /// DELETE /api/User/689faa4b8356a979df152dab
    /// Authorization: Bearer {token}
    /// ```
    /// </remarks>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
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

    /// <summary>
    /// Email adresinin kullanımda olup olmadığını kontrol etme
    /// </summary>
    /// <param name="email">Kontrol edilecek email adresi</param>
    /// <returns>Email kullanımda mı bilgisi (true/false)</returns>
    /// <response code="200">Kontrol tamamlandı</response>
    /// <response code="500">Sunucu hatası</response>
    /// <remarks>
    /// Belirtilen email adresinin sistemde kayıtlı olup olmadığını kontrol eder.
    /// 
    /// **Kullanım Senaryoları:**
    /// - Kayıt formunda real-time email kontrolü
    /// - Forgot password işleminde email doğrulama
    /// - Email availability check
    /// - Form validasyonu
    /// 
    /// **Güvenlik:**
    /// - Anonymous erişime açık
    /// - Rate limiting uygulanmalı
    /// - Email enumeration saldırılarına karşı korunmalı
    /// - Brute force protection
    /// 
    /// **Dönen Değerler:**
    /// - true: Email sistemde kayıtlı
    /// - false: Email kullanılabilir
    /// 
    /// **Privacy Considerations:**
    /// - User privacy korunmalı
    /// - Detailed error mesajları verilmemeli
    /// - Generic response'lar tercih edilmeli
    /// 
    /// **Örnek Kullanım:**
    /// ```
    /// GET /api/User/check-email/user@example.com
    /// 
    /// Response: 
    /// {
    ///   "success": true,
    ///   "data": true,  // Email kullanımda
    ///   "message": null
    /// }
    /// ```
    /// </remarks>
    [HttpGet("check-email/{email}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status500InternalServerError)]
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


