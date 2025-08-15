using Microsoft.OpenApi.Models;

namespace API.Configuration;

public static class OpenApiConfiguration
{
    public static void ConfigureOpenApi(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
            
            // XML Documentation ekle
            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                options.AddDocumentTransformer((document, context, cancellationToken) =>
                {
                    document.Info.Title = "Prodicts API";
                    document.Info.Description = "Dil öğrenimi için kelime ve flashcard yönetim API'si";
                    document.Info.Version = "v1";
                    document.Info.Contact = new OpenApiContact
                    {
                        Name = "Prodicts API Support",
                        Email = "support@prodicts.com"
                    };
                    
                    // Controller bazlı tag açıklamaları ekle
                    document.Tags = GetApiTags();
                    
                    return Task.CompletedTask;
                });
            }
        });
    }
    
    private static List<OpenApiTag> GetApiTags()
    {
        return new List<OpenApiTag>
        {
            new OpenApiTag
            {
                Name = "Auth",
                Description = @"🔐 **Authentication ve Authorization Endpoint'leri**

Bu controller'da tüm giriş, kayıt ve token yönetimi işlemleri bulunur.

**🎯 Ana Özellikler:**
• Traditional Login/Register (Email/Password)
• OAuth Authentication (Google, Facebook, Apple)  
• Anonymous User Authentication
• JWT Token Management
• Refresh Token Operations

**📱 Desteklenen Authentication Yöntemleri:**

**Traditional:** Email ve şifre ile klasik giriş
**OAuth:** Sosyal medya hesapları ile giriş  
**Anonymous:** Device ID ile kayıt olmadan kullanım

**⏱️ Token Süresi:**
• Registered Users: 60 dakika
• Anonymous Users: 24 saat
• Refresh Token: 30 gün

**🚀 Hızlı Başlangıç:**
1. Anonymous token için: `POST /api/Auth/anonymous`
2. Kayıt için: `POST /api/Auth/register`
3. Giriş için: `POST /api/Auth/login`"
            },
            new OpenApiTag
            {
                Name = "User",
                Description = @"👤 **Kullanıcı Yönetimi Endpoint'leri**

Bu controller'da kullanıcı CRUD işlemleri ve özel kullanıcı operasyonları bulunur.

**🛠️ Ana Özellikler:**
• Kullanıcı Profil Yönetimi
• OAuth Provider Registration
• Anonymous User Management  
• Data Synchronization
• User Upgrade Operations

**📊 Kullanıcı Tipleri:**

**Registered:** Email/şifre veya OAuth ile kayıtlı kullanıcılar
**Anonymous:** Device ID ile takip edilen geçici kullanıcılar

**🔄 Özel İşlemler:**
• Anonymous→Registered dönüşümü
• Multi-device data sync
• Provider-based registration
• Email availability check

**🔒 Güvenlik:**
• Varsayılan olarak tüm endpoint'ler korumalı
• Sadece public endpoint'ler anonymous erişime açık
• User data privacy korunur"
            }
        };
    }
}
