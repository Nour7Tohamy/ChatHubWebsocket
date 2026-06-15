using Application;
using Infrastructure.Data;
using Infrastructure.ServiceExtensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Presentation.Middlewares;
using PresentationMVC;
using Serilog;
using System.Text;

public partial class Program
{
    private static async Task Main(string[] args)
    {
        // ════ Bootstrap Serilog early (قبل builder) ════
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        try
        {
            Log.Information("Starting ChatHub...");

            var builder = WebApplication.CreateBuilder(args);

            // ════ SERILOG ════
            builder.Host.UseSerilog((context, services, config) =>
                config.ReadFrom.Configuration(context.Configuration)
                      .ReadFrom.Services(services)
                      .Enrich.FromLogContext());

            // ════ SERVICES ════
            builder.Services.AddApplication();
            builder.Services.AddInfrastructure(builder.Configuration);
            builder.Services.AddControllersWithViews();
            builder.Services.AddSignalR();
            builder.Services.AddHttpContextAccessor();

            // ════ CORS ════
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", policy =>
                    policy.WithOrigins("https://localhost:7001", "http://localhost:5001")
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials());
            });

            // ════ AUTH ════
            // AddIdentity في InfrastructureServiceExtensions بيسجل Cookie scheme تلقائياً
            // بنضيف JWT فوقيه للـ SignalR بس — من غير ما نـ override الـ DefaultScheme
            builder.Services.AddAuthentication()
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
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(
                                builder.Configuration["Jwt:Key"]
                                ?? throw new InvalidOperationException("JWT Key missing")))
                    };

                    // SignalR — يجيب الـ token من query string
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
                        }
                    };
                });

            // Cookie redirect — لما [Authorize] يرد المتصفح لصفحة Login
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Auth/Login";
                options.AccessDeniedPath = "/Auth/Login";
                options.ExpireTimeSpan = TimeSpan.FromDays(7);
                options.SlidingExpiration = true;
            });

            builder.Services.AddAuthorization();

            var app = builder.Build();

            // ════ AUTO MIGRATION ════
            try
            {
                using var scope = app.Services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                Log.Information("Running EF migrations...");
                await db.Database.MigrateAsync();
                Log.Information("Migrations completed successfully");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Migration failed");
            }
            // ════ RESET ONLINE STATUS ON STARTUP ════
            try
            {
                using var scope = app.Services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await db.Users
                    .Where(u => u.IsOnline)
                    .ExecuteUpdateAsync(s => s.SetProperty(u => u.IsOnline, false));
                Log.Information("Online status reset completed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to reset online status");
            }
            // ════ MIDDLEWARE PIPELINE ════
            app.UseHttpsRedirection();

            app.UseSerilogRequestLogging(opts =>
            {
                opts.MessageTemplate =
                    "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0}ms";
            });

            app.UseMiddleware<GlobalExceptionMiddleware>();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseCors("CorsPolicy");
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapHub<ChatHub>("/chathub");

            await app.RunAsync();
        }
        catch (Exception ex) when (ex is not HostAbortedException)
        {
            Log.Fatal(ex, "ChatHub terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}