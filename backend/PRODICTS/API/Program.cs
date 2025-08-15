using API.Configuration;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Configuration
builder.Services.ConfigureServices(builder.Configuration);
builder.Services.ConfigureAuthentication(builder.Configuration);
builder.Services.ConfigureOpenApi();
builder.Services.ConfigureCors();

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.ConfigureScalar();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
