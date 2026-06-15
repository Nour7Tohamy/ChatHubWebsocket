using Application.Infrastructure.Repositories;
using Application.Infrastructure.Services;
using Application.Infrastructure.Services.Messages;
using Domain.Entities.Main;
using Infrastructure.Data;
using Infrastructure.Infrastructure.Repository;
using Infrastructure.Infrastructure.Services.Messages;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
        // AddIdentity بيسجل Cookie Authentication scheme تلقائياً
        // ده اللي بيخلي [Authorize] على MVC pages يشتغل صح
        services.AddIdentity<AppUser, IdentityRole>(options =>
        {
            options.Password.RequiredLength = 6;
            options.Password.RequireDigit = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = true;

            // منع lockout في التطوير لو حبيت تعطله
            // options.Lockout.AllowedForNewUsers = false;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        // ════ Repositories / UnitOfWork ════
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // ════ Services ════
        services.AddScoped<IMessageService, MessageService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        return services;
    }
}