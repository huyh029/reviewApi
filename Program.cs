using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Mvc;
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
// Cho phép tự kiểm soát ModelState thay vì auto-400
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

// DB PostgreSQL: dùng trực tiếp connection string cố định (không đọc environment)
var connectionString = "Host=dpg-d4ruumi4d50c73b4jt60-a;Port=5432;Database=huyh0;Username=huyh0_user;Password=WiPoRZzAnr4aeJNiDEqh2OMVEld6f4oz;SslMode=Require;TrustServerCertificate=true";

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
        // Fixed JWT settings (không đọc environment)
        var key = "supersecretkey1234567890abcdefxyz!";
        var issuer = "uerManage";
        var audience = "uerManage";

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
