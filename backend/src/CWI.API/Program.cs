using CWI.Application;
using CWI.Infrastructure;
using CWI.Infrastructure.Persistence;
using CWI.API.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using OfficeOpenXml;

#pragma warning disable CS0618
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
#pragma warning restore CS0618

var builder = WebApplication.CreateBuilder(args);

// Fix for 431 Request Header Fields Too Large
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestHeadersTotalSize = 64 * 1024; // 64KB
    options.Limits.MaxRequestBodySize = 52428800; // 50MB (Excel upload icin artiralim)
});

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache(options =>
{
    var cachingSection = builder.Configuration.GetSection("Caching");
    var sizeLimitMb = cachingSection.GetValue<long?>("MemorySizeLimitMb") ?? 64;
    var compactionPercentage = cachingSection.GetValue<double?>("CompactionPercentage") ?? 0.2;

    options.SizeLimit = sizeLimitMb * 1024 * 1024;
    options.CompactionPercentage = compactionPercentage;
    options.ExpirationScanFrequency = TimeSpan.FromMinutes(1);
});
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

// Swagger Konfigurasyonu (JWT destegi ile)
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CWI API",
        Version = "v1",
        Description = "CWI Backend API - Clean Architecture"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\""
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// JWT Kimlik Dogrulama Kaydi
var jwtKey = builder.Configuration["Jwt:Key"] ?? "CWI_Secret_Key_At_Least_32_Chars_Long";
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});
builder.Services.AddAuthorization();

// Application ve Infrastructure katmanlarini ekle
builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

// CORS ayarlari
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Global Exception Middleware (Pipeline'in en basinda olmali)
app.UseMiddleware<ExceptionMiddleware>();
app.UsePathBase("/api");

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "CWI API v1");
    });
}

app.UseHttpsRedirection();
app.UseResponseCompression();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseCors("AllowAll");

app.UseAuthentication(); // Auth her zaman Authorization'dan once gelmeli
app.UseAuthorization();

app.MapControllers();
app.MapFallbackToFile("index.html");

// Veritabani Seed Islemi
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<CWIDbContext>();
        await CWIDbContextSeed.SeedAsync(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Veritabani seed edilirken bir hata olustu.");
    }
}

// API Saglik Kontrolu (Health Check)
app.MapGet("/health", () => Results.Ok(new { status = "Healthy", time = DateTime.UtcNow }));

app.Run();
