using Scalar.AspNetCore;

namespace API.Configuration;

public static class ScalarConfiguration
{
    public static void ConfigureScalar(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference(options =>
            {
                options.Title = "Prodicts API";
                options.Theme = ScalarTheme.Default;
                options.ShowSidebar = true;
            });
            
            app.UseCors("DevPolicy");
            
            // Root'dan Scalar'a yönlendirme
            app.MapGet("/", () => Results.Redirect("/scalar/v1"));
        }
    }
}
