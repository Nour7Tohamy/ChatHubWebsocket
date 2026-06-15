using Application.Infrastructure.Repositories;
using Application.Infrastructure.Services;
using Application.Infrastructure.Services.Messages;
using Application.Infrastructure.Services.Notifitions;
using Domain.Entities.Main;
using Infrastructure.Data;
using Infrastructure.Infrastructure.Repository;
using Infrastructure.Infrastructure.Services;
using Infrastructure.Infrastructure.Services.Messages;
using Infrastructure.Infrastructure.Services.Notifications;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Infrastructure.ServiceExtensions;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ════ DbContext ════
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql => sql.EnableRetryOnFailure()));

        // ════ Identity ════
        services.AddIdentity<AppUser, IdentityRole>(options =>
        {
            options.Password.RequiredLength = 6;
            options.Password.RequireDigit = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        // ════ Cookie config ════
        // AddIdentity بيعمل override على DefaultScheme،
        // فلازم نعيد تعريف الـ Cookie options هنا بعده مباشرة
        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/Auth/Login";
            options.AccessDeniedPath = "/Auth/AccessDenied";
            options.ExpireTimeSpan = TimeSpan.FromDays(7);
            options.SlidingExpiration = true;

            options.Events.OnRedirectToLogin = ctx =>
            {
                if (ctx.Request.Path.StartsWithSegments("/api"))
                {
                    ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                }
                ctx.Response.Redirect(ctx.RedirectUri);
                return Task.CompletedTask;
            };

            options.Events.OnRedirectToAccessDenied = ctx =>
            {
                if (ctx.Request.Path.StartsWithSegments("/Admin"))
                {
                    ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                }
                ctx.Response.Redirect(ctx.RedirectUri);
                return Task.CompletedTask;
            };
        });

        // ════ JWT ════
        services.AddAuthentication()
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
         options.TokenValidationParameters = new TokenValidationParameters
         {
             ValidateIssuer = true,
             ValidateAudience = true,
             ValidateLifetime = true,
             ValidateIssuerSigningKey = true,
             ValidIssuer = configuration["Jwt:Issuer"],
             ValidAudience = configuration["Jwt:Audience"],
             IssuerSigningKey = new SymmetricSecurityKey(
                 Encoding.UTF8.GetBytes(
                     configuration["Jwt:Key"]
                     ?? throw new InvalidOperationException(
                         "JWT Key missing — set it as an environment variable: Jwt__Key")))
         };

         options.Events = new JwtBearerEvents
         {
             OnMessageReceived = ctx =>
             {
                 var accessToken = ctx.Request.Query["access_token"];
                 var path = ctx.HttpContext.Request.Path;
                 if (!string.IsNullOrEmpty(accessToken) &&
                     path.StartsWithSegments("/chathub"))
                     ctx.Token = accessToken;
                 return Task.CompletedTask;
             },

             OnChallenge = ctx =>
             {
                 ctx.HandleResponse();
                 ctx.Response.StatusCode = 401;
                 ctx.Response.ContentType = "application/json";
                 var body = System.Text.Json.JsonSerializer.Serialize(
                     new { error = "Unauthorized — invalid or missing JWT token." });
                 return ctx.Response.WriteAsync(body);
             }
         };
     });

        // ════ Repositories / UnitOfWork ════
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // ════ Services ════
        services.AddScoped<IMessageService, MessageService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<ICookieAuthService, CookieAuthService>();
        services.AddScoped<INotificationService, NotificationService>();
        return services;
    }
}