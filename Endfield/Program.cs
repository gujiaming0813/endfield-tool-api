using System.Text;
using Endfield.Api.Data;
using Endfield.Api.Services;
using Endfield.Api.Share.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpClient();

// 添加内存缓存
builder.Services.AddMemoryCache();

// 配置JWT选项
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

// 配置JWT认证
var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>();
if (jwtOptions == null)
{
    throw new InvalidOperationException("JWT配置缺失，请在appsettings.json中配置Jwt节点");
}

// 注册Token缓存服务（需要在AddAuthentication之前注册）
builder.Services.AddSingleton<ITokenCacheService, TokenCacheService>();

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
        ValidIssuer = jwtOptions.Issuer,
        ValidAudience = jwtOptions.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
        ClockSkew = TimeSpan.Zero
    };

    // 自定义Token验证事件
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var tokenCache = context.HttpContext.RequestServices.GetRequiredService<ITokenCacheService>();
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();

            var userIdClaim = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var jtiClaim = context.Principal?.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                logger.LogWarning("Token中缺少用户ID声明");
                context.Fail("无效的Token");
                return;
            }

            // 从请求头获取当前Token
            var authorizationHeader = context.Request.Headers.Authorization.FirstOrDefault();
            if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
            {
                context.Fail("缺少Token");
                return;
            }

            var currentToken = authorizationHeader["Bearer ".Length..].Trim();

            // 验证Token是否与缓存中的一致
            var isValid = await tokenCache.ValidateTokenAsync(userId, currentToken);
            if (!isValid)
            {
                logger.LogWarning("Token已失效，用户ID: {UserId}", userId);
                context.Fail("Token已失效，请重新登录");
            }
        }
    };
});

builder.Services.AddAuthorization();

// 配置Swagger（支持JWT认证）
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Endfield API", Version = "v1" });

    // 添加JWT认证支持
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "请输入JWT令牌，格式：Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
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
            []
        }
    });
});

// 配置MySQL数据库连接
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var serverVersion = new MySqlServerVersion(new Version(8, 0, 0));
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, serverVersion));

// 注册服务
builder.Services.AddScoped<IBilibiliService, BilibiliService>();
builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// 添加CORS支持
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
