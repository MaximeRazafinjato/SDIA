using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;
using SDIA.API.Data;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/sdia-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    
    // Add Serilog
    builder.Host.UseSerilog();

    // Add services to the container.
    // Database configuration - using SQL Server for persistence
    builder.Services.AddDbContext<SimpleSDIADbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
    
    // CORS for development
    if (builder.Environment.IsDevelopment())
    {
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("DevelopmentPolicy",
                policy => policy
                    .WithOrigins("https://localhost:5173", "http://localhost:5173")
                    .AllowCredentials()
                    .AllowAnyHeader()
                    .AllowAnyMethod());
        });
    }
    
    // Authentication
    builder.Services
        .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
        {
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.None;
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
            options.SlidingExpiration = true;
            options.LoginPath = "/api/auth/login";
            options.LogoutPath = "/api/auth/logout";
            options.AccessDeniedPath = "/api/auth/access-denied";
            
            // Return 401 for API calls instead of redirecting
            options.Events.OnRedirectToLogin = context =>
            {
                if (context.Request.Path.StartsWithSegments("/api"))
                {
                    context.Response.StatusCode = 401;
                }
                else
                {
                    context.Response.Redirect(context.RedirectUri);
                }
                return Task.CompletedTask;
            };
        });
    
    builder.Services.AddAuthorization();
    
    // Add application services (temporarily commented out due to compilation errors)
    // builder.Services.AddApplicationServices();
    // builder.Services.AddInfrastructureServices(builder.Configuration);
    
    // Register email service
    builder.Services.AddScoped<SDIA.Core.Services.IEmailService, SDIA.API.Services.SimpleEmailService>();
    
    // Controllers
    builder.Services.AddControllers()
        .AddNewtonsoftJson(options =>
        {
            options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
        });
    
    // OpenAPI
    builder.Services.AddOpenApi();
    
    var app = builder.Build();
    
    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseCors("DevelopmentPolicy");
        app.MapOpenApi();
        app.MapScalarApiReference(options =>
        {
            options.Title = "SDIA API Documentation";
            options.Theme = ScalarTheme.DeepSpace;
            options.DarkMode = true;
        });
    }
    
    app.UseHttpsRedirection();
    app.UseSerilogRequestLogging();
    
    app.UseAuthentication();
    app.UseAuthorization();
    
    app.MapControllers();
    
    // Database initialization
    using (var scope = app.Services.CreateScope())
    {
        try
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<SimpleSDIADbContext>();
            
            // Apply migrations for SQL Server - commented since migrations are already applied
            // await dbContext.Database.MigrateAsync();
            
            // Initialize with seed data
            await SimpleDbInitializer.InitializeAsync(scope.ServiceProvider);
            
            Log.Information("Database initialized successfully with SQL Server");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while initializing the database");
        }
    }
    
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}