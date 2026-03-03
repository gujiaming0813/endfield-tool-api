using System.Text;
using Endfield.Api.Data;
using Endfield.Api.Services;
using Endfield.Api.Share.Options;
using Endfield.Api.Filters;
using Endfield.Api.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using Serilog;
using Serilog.Events;
using Hangfire;
using Hangfire.MySql;

// 初始化Serilog日志（最早初始化以捕获所有日志）
var serilogOptions = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true, reloadOnChange: true)
    .Build()
    .GetSection("Serilog")
    .Get<SerilogOptions>() ?? new SerilogOptions();

// 构建Serilog日志配置
var loggerConfiguration = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", serilogOptions.ApplicationName)
    .Enrich.WithProperty("Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production")
    .Enrich.WithProperty("MachineName", Environment.MachineName);

// 设置最小日志级别
var minLevel = Enum.Parse<LogEventLevel>(serilogOptions.MinimumLevel, true);
loggerConfiguration.MinimumLevel.Is(minLevel);

// 配置控制台输出
if (serilogOptions.Console.Enabled)
{
    if (serilogOptions.OutputJsonFormat)
    {
        // JSON格式输出，适合日志收集系统
        loggerConfiguration.WriteTo.Console(new Serilog.Formatting.Compact.CompactJsonFormatter());
    }
    else
    {
        loggerConfiguration.WriteTo.Console(outputTemplate: serilogOptions.Console.OutputTemplate);
    }
}

// 配置文件输出
if (serilogOptions.File.Enabled)
{
    var rollingInterval = Enum.Parse<RollingInterval>(serilogOptions.File.RollingInterval, true);

    if (serilogOptions.OutputJsonFormat)
    {
        // JSON格式输出到文件，适合日志收集系统
        loggerConfiguration.WriteTo.File(
            new Serilog.Formatting.Compact.CompactJsonFormatter(),
            serilogOptions.File.Path,
            rollingInterval: rollingInterval,
            retainedFileCountLimit: serilogOptions.File.RetainedFileCountLimit,
            fileSizeLimitBytes: serilogOptions.File.FileSizeLimitMb * 1024 * 1024,
            rollOnFileSizeLimit: true
        );
    }
    else
    {
        loggerConfiguration.WriteTo.File(
            serilogOptions.File.Path,
            outputTemplate: serilogOptions.File.OutputTemplate,
            rollingInterval: rollingInterval,
            retainedFileCountLimit: serilogOptions.File.RetainedFileCountLimit,
            fileSizeLimitBytes: serilogOptions.File.FileSizeLimitMb * 1024 * 1024,
            rollOnFileSizeLimit: true
        );
    }
}

// 创建Serilog日志实例
Log.Logger = loggerConfiguration.CreateLogger();

try
{
    Log.Information("正在启动应用 {ApplicationName}", serilogOptions.ApplicationName);

    var builder = WebApplication.CreateBuilder(args);

    // 使用Serilog作为日志提供程序
    builder.Services.AddSerilog();

    // Add services to the container.
    builder.Services.AddControllers(options =>
    {
        // 添加全局日志过滤器
        options.Filters.Add<LogActionFilter>();
        options.Filters.Add<GlobalExceptionFilter>();
    });
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

    // 配置 Hangfire（使用 MySQL 存储）
    builder.Services.AddHangfire(config => config
        .UseStorage(new MySqlStorage(connectionString, new MySqlStorageOptions
        {
            TransactionIsolationLevel = System.Transactions.IsolationLevel.ReadCommitted,
            QueuePollInterval = TimeSpan.FromSeconds(15),
            JobExpirationCheckInterval = TimeSpan.FromHours(1),
            CountersAggregateInterval = TimeSpan.FromMinutes(5),
            PrepareSchemaIfNecessary = true,
            DashboardJobListLimit = 50000,
            TransactionTimeout = TimeSpan.FromMinutes(1),
            TablesPrefix = "Hangfire"
        })));

    // 添加 Hangfire 服务器
    builder.Services.AddHangfireServer();

    // 注册服务
    builder.Services.AddScoped<IBilibiliService, BilibiliService>();
    builder.Services.AddScoped<ITagService, TagService>();
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IRequestLogService, RequestLogService>();
    builder.Services.AddScoped<IVideoRefreshService, VideoRefreshService>();

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

    // 配置 Hangfire Dashboard（仅在开发环境启用）
    if (app.Environment.IsDevelopment())
    {
        app.UseHangfireDashboard("/hangfire");
    }

    // 配置定时任务：每天凌晨1点刷新近一个月的视频
    RecurringJob.AddOrUpdate<IVideoRefreshService>(
        "RefreshRecentVideos",
        service => service.RefreshRecentVideosAsync(CancellationToken.None),
        "0 1 * * *",
        new RecurringJobOptions { TimeZone = TimeZoneInfo.Local });

    app.MapControllers();

    Log.Information("应用启动完成，正在监听端口");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "应用启动失败");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
