using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OfficeOpenXml;
using reviewApi.Models;
using reviewApi.Service;
using reviewApi.Service.Repositories;
using System.Text;
using System.Text.Json;


var builder = WebApplication.CreateBuilder(args);

ExcelPackage.License.SetNonCommercialPersonal("YourName");
// ----------------- Add services -----------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger với JWT
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Nhập JWT: Bearer {token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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

// CORS mở cho mọi origin (cân nhắc thu hẹp khi cần)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// DB PostgreSQL với URL (chỉ đọc từ biến môi trường)
string BuildConnectionString()
{
    var raw = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
              ?? Environment.GetEnvironmentVariable("DATABASE_URL")
              ?? Environment.GetEnvironmentVariable("DEFAULT_DATABASE_URL")
              // Fallback hard-coded for testing/deploy (replace if needed)
              ?? "postgresql://huyh0_user:WiPoRZzAnr4aeJNiDEqh2OMVEld6f4oz@dpg-d4ruumi4d50c73b4jt60-a.virginia-postgres.render.com:5432/huyh0";

    if (string.IsNullOrWhiteSpace(raw))
        throw new Exception("Missing database connection string. Set ConnectionStrings__DefaultConnection or DATABASE_URL.");

    raw = raw.Trim();

    if (raw.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
        raw.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
    {
        // Không kiểm tra chặt chẽ; để Npgsql tự xử lý URL đặc biệt
        try
        {
            var csBuilder = new Npgsql.NpgsqlConnectionStringBuilder(raw);
            return csBuilder.ConnectionString;
        }
        catch
        {
            // Nếu builder không parse được thì trả lại nguyên bản
            return raw;
        }
    }

    return raw;
}

var connectionString = BuildConnectionString();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));


builder.Services.AddTransient(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddTransient<IUnitOfWork, UnitOfWork>();
builder.Services.AddTransient<IEvaluationService, EvaluationService>();
builder.Services.AddTransient<IService, Service>();
builder.Services.AddTransient<IReportService, ReportService>();
builder.Services.AddTransient<ICriteriaSetService, CriteriaSetService>();
builder.Services.AddTransient<IAuthService, AuthService>();
builder.Services.AddTransient<IEvaluationFlowService, EvaluationFlowService>();
builder.Services.AddTransient<IEvaluationObjectService, EvaluationObjectService>();
builder.Services.AddTransient<IDashboardService, DashboardService>();
// Nếu muốn logout với blacklist → dùng MemoryCache trước
builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();

// ----------------- Authentication + JWT -----------------
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        string GetJwtSetting(string key) =>
            Environment.GetEnvironmentVariable($"JwtSettings__{key}") ?? string.Empty;

        var key = GetJwtSetting("Key");
        if (string.IsNullOrWhiteSpace(key))
            throw new Exception("JWT secret key is missing or empty! Set JwtSettings__Key environment variable.");

        var issuer = GetJwtSetting("Issuer");
        var audience = GetJwtSetting("Audience");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.Zero, // bỏ 5 phút grace mặc định
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
        };

        // Nếu bạn muốn check blacklist ngay trong quá trình xác thực:
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var cache = context.HttpContext.RequestServices.GetRequiredService<IMemoryCache>();
                var jti = context.Principal.FindFirst("jti")?.Value;
                if (jti != null && cache.TryGetValue($"blacklist_{jti}", out _))
                {
                    context.Fail("Token is blacklisted!");
                }
            }
        };
    });


// ----------------- App Pipeline -----------------
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred creating the DB.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseExceptionHandler(config =>
{
    config.Run(async context =>
    {
        context.Response.StatusCode = 400; // hoặc 500 tùy trường hợp
        context.Response.ContentType = "application/json";

        var error = context.Features.Get<IExceptionHandlerFeature>();
        if (error != null)
        {
            var result = JsonSerializer.Serialize(new
            {
                message = error.Error.Message
            });
            await context.Response.WriteAsync(result);
        }
    });
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
