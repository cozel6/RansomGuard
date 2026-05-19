using Microsoft.EntityFrameworkCore;
using PeNet.Header.Resource;
using RansomGuard.API.Data;
using RansomGuard.API.Middlewares;
using RansomGuard.API.Services;
using Serilog;
using Serilog.Events;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        path: "logs/ransomguard-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Starting RansomGuard API");

    var builder = WebApplication.CreateBuilder(args);

    // Replace default logging with Serilog
    builder.Host.UseSerilog();

    // Add services
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
            options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));

    // Add Swagger
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
            options.IncludeXmlComments(xmlPath);
    });

    // Add response caching
    builder.Services.AddResponseCaching();


    builder.Services.AddDbContext<RansomGuardDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

    // Add repositories
    builder.Services.AddScoped<IAnalysisRepository, AnalysisRepository>();

    // Add file upload helper
    builder.Services.AddScoped<IFileUploadHelper, FileUploadHelper>();

    // Add PE analysis service
    builder.Services.AddScoped<IPEAnalysisService, PEAnalysisService>();

    // Add CORS
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            var allowedOrigins = builder.Configuration.GetSection("RansomGuard:AllowedOrigins").Get<string[]>()
                ?? ["http://localhost:3000"];

            policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });
    // Add ML service client
    builder.Services.AddHttpClient<IMlServiceClient, MlServiceClient>(client =>
    {
        var baseUrl = builder.Configuration["MlService:BaseUrl"] ?? "http://localhost:8000";
        client.BaseAddress = new Uri(baseUrl);
        client.Timeout = TimeSpan.FromSeconds(30);
    });


    var app = builder.Build();

    // Apply pending migrations
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<RansomGuardDbContext>();
        await db.Database.MigrateAsync();
    }

    // Configure middleware
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }
    app.UseMiddleware<ErrorHandlingMiddleware>();
    app.UseHttpsRedirection();
    app.UseResponseCaching();
    app.UseCors();

    // Redirect root to Swagger
    app.MapGet("/", () => Results.Redirect("/swagger"))
       .ExcludeFromDescription();

    // Map controllers
    app.MapControllers();

    // Health check endpoint
    app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
       .WithName("HealthCheck");

    Log.Information("RansomGuard API started successfully");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application failed to start");
    throw;
}
finally
{
    Log.CloseAndFlush();
}