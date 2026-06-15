    using Application;
    using Infrastructure.Data;
    using Infrastructure.ServiceExtensions;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.HttpOverrides;
    using Presentation.Middlewares;
    using PresentationMVC;
    using PresentationMVC.Seeders;
    using Serilog;

    public partial class Program
    {
        private static async Task Main(string[] args)
        {
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
                builder.Services.AddResponseCompression();

                // ════ CORS ════
                var allowedOrigins = builder.Configuration
                    .GetSection("Cors:AllowedOrigins")
                    .Get<string[]>() ?? Array.Empty<string>();

                builder.Services.AddCors(options =>
                {
                    options.AddPolicy("CorsPolicy", policy =>
                    {
                        if (builder.Environment.IsDevelopment())
                            policy.SetIsOriginAllowed(o => new Uri(o).Host == "localhost")
                                  .AllowAnyHeader().AllowAnyMethod().AllowCredentials();
                        else
                            policy.WithOrigins(allowedOrigins)
                                  .AllowAnyHeader().AllowAnyMethod().AllowCredentials();
                    });
                });

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("JwtPolicy", policy =>
                    policy
                        .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                        .RequireAuthenticatedUser());
            });

            var app = builder.Build();

                // ════ AUTO MIGRATION ════
                try
                {
                    using var scope = app.Services.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    Log.Information("Running EF migrations...");
                    await db.Database.MigrateAsync();
                    Log.Information("Migrations completed");
                }
                catch (Exception ex) { Log.Error(ex, "Migration failed"); }

                // ════ RESET ONLINE STATUS ════
                try
                {
                    using var scope = app.Services.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    await db.Users.Where(u => u.IsOnline)
                        .ExecuteUpdateAsync(s => s.SetProperty(u => u.IsOnline, false));
                    Log.Information("Online status reset");
                }
                catch (Exception ex) { Log.Error(ex, "Failed to reset online status"); }

                // ════ SEED ROLES ════
                try
                {
                    using var scope = app.Services.CreateScope();
                    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                    await RoleSeeder.SeedAsync(roleManager);
                    Log.Information("Roles seeded");
                }
                catch (Exception ex) { Log.Error(ex, "Failed to seed roles"); }

                // ════ ASSIGN FIRST ADMIN ════
                try
                {
                    using var scope = app.Services.CreateScope();
                    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
                    var firstAdminEmail = app.Configuration["Admin:FirstAdminEmail"];
                    if (!string.IsNullOrWhiteSpace(firstAdminEmail))
                    {
                        var adminUser = await userManager.FindByEmailAsync(firstAdminEmail);
                        if (adminUser is not null && !await userManager.IsInRoleAsync(adminUser, "Admin"))
                        {
                            await userManager.AddToRoleAsync(adminUser, "Admin");
                            Log.Information("Admin role assigned to {Email}", firstAdminEmail);
                        }
                    }
                }
                catch (Exception ex) { Log.Error(ex, "Failed to assign first admin"); }

                // ════ MIDDLEWARE PIPELINE ════
                app.UseForwardedHeaders(new ForwardedHeadersOptions
                {
                    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
                });

                app.UseHsts();
                app.UseHttpsRedirection();
                app.UseResponseCompression();

                app.UseSerilogRequestLogging(opts =>
                    opts.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0}ms");

                app.UseMiddleware<GlobalExceptionMiddleware>();

                app.UseStaticFiles(new StaticFileOptions
                {
                    OnPrepareResponse = ctx =>
                    {
                        var path = ctx.File.PhysicalPath ?? "";
                        var maxAge = path.Contains("/voice/") ? "86400" : "604800";
                        ctx.Context.Response.Headers.Append("Cache-Control", $"public, max-age={maxAge}");
                    }
                });

                app.UseRouting();
                app.UseCors("CorsPolicy");
                app.UseAuthentication();
                app.UseAuthorization();

                app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
                app.MapControllers();
                app.MapHub<ChatHub>("/chathub")
                   .RequireAuthorization(policy =>
                       policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                             .RequireAuthenticatedUser());

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