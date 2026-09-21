using System.Text;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Storage.Infrastructure.Data;
using Storage.Infrastructure.Services;
using Storage.Infrastructure.Clients;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Storage.Api", Version = "v1" }));

builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContext<StorageDbContext>(o =>
    o.UseNpgsql(builder.Configuration.GetConnectionString("StorageDatabase")));

builder.Services.AddScoped<FileStorageService>();
builder.Services.AddScoped<PermissionService>();
builder.Services.AddScoped<ActivityLogger>();

// HTTP Client برای Identity
builder.Services.AddHttpClient<IdentityClient>(c =>
{
    c.BaseAddress = new Uri(builder.Configuration["Services:IdentityUrl"] ?? "http://localhost:5002");
    c.Timeout = TimeSpan.FromSeconds(10);
});

var jwt = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o => o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, ValidateAudience = true, ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwt["Issuer"], ValidAudience = jwt["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Secret"]!))
    });

builder.Services.AddFluentValidationAutoValidation();

var app = builder.Build();

if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }

// سرو فایل‌های آپلود شده
var uploadPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
    "Downloads", "amazon-clone", "storage_uploads");
Directory.CreateDirectory(uploadPath);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadPath),
    RequestPath = "/storage"
});

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Seed permissions on startup
using (var scope = app.Services.CreateScope())
{
    var permService = scope.ServiceProvider.GetRequiredService<PermissionService>();
    try { await permService.SeedDefaultPermissionsAsync(); }
    catch (Exception ex) { Console.WriteLine($"Seed error: {ex.Message}"); }
}

app.Run();
