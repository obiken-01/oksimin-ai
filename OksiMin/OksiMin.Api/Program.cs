using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using OksiMin.Api.Extensions;
using OksiMin.Api.Filters;
using OksiMin.Application.Interfaces;
using OksiMin.Application.Services;
using OksiMin.Application.Validators;
using OksiMin.Infrastructure.Data;
using Serilog;
using Serilog.Events;

// ============================================
// STEP 1: Configure Serilog
// ============================================
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .Enrich.WithEnvironmentName()
    .Enrich.WithProperty("Application", "OksiMin.Api")
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File(
        path: "Logs/oksimin-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File(
        formatter: new Serilog.Formatting.Compact.CompactJsonFormatter(),
        path: "Logs/oksimin-structured-.json",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30)
    .WriteTo.Seq(
        serverUrl: "http://localhost:5341",
        restrictedToMinimumLevel: LogEventLevel.Debug)
    .CreateLogger();

try
{
    Log.Information("========================================");
    Log.Information("Starting OksiMin.Api application");
    Log.Information("========================================");

    var builder = WebApplication.CreateBuilder(args);

    // Replace default logging with Serilog
    builder.Host.UseSerilog();

    // ============================================
    // STEP 2: Add services
    // ============================================
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "OksiMin.Api",
            Version = "v1",
            Description = "AI-powered local information system for Occidental Mindoro, Philippines",
            Contact = new OpenApiContact
            {
                Name = "OksiMin.Ai",
                Email = "contact@oksimin.ai"
            }
        });

        // Add Correlation ID to Swagger
        c.OperationFilter<CorrelationIdOperationFilter>();

        // Enable XML comments
        var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            c.IncludeXmlComments(xmlPath);
        }
    });

    // Database
    builder.Services.AddDbContext<OksiMinDbContext>(options =>
    {
        options.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection"),
            sqlOptions => sqlOptions.EnableRetryOnFailure());

        if (builder.Environment.IsDevelopment())
        {
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
        }
    });

    // Register DbContext interface for Application layer
    builder.Services.AddScoped<IApplicationDbContext>(provider =>
        provider.GetRequiredService<OksiMinDbContext>());

    // Application Services
    builder.Services.AddScoped<ISubmissionService, SubmissionService>();
    builder.Services.AddScoped<ICategoryService, CategoryService>();
    builder.Services.AddScoped<IPlaceService, PlaceService>();

    // Validators
    builder.Services.AddValidatorsFromAssemblyContaining<CreateSubmissionValidator>();

    // CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowFrontend", policy =>
        {
            policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .WithExposedHeaders("X-Correlation-ID"); // Expose correlation ID to frontend
        });
    });

    // HttpContext accessor for accessing HttpContext in services
    builder.Services.AddHttpContextAccessor();

    var app = builder.Build();

    // ============================================
    // STEP 3: Configure middleware pipeline
    // IMPORTANT: Order matters!
    // ============================================

    // 1. Correlation ID (FIRST - before any other middleware)
    app.UseCorrelationId();

    // 2. Global Exception Handler (SECOND - catches all exceptions)
    app.UseGlobalExceptionHandler();

    // 3. Serilog Request Logging (THIRD - logs all requests with correlation ID)
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";

        options.GetLevel = (httpContext, elapsed, ex) => ex != null
            ? LogEventLevel.Error
            : httpContext.Response.StatusCode > 499
                ? LogEventLevel.Error
                : httpContext.Response.StatusCode > 399
                    ? LogEventLevel.Warning
                    : LogEventLevel.Information;

        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
            diagnosticContext.Set("RemoteIP", httpContext.Connection.RemoteIpAddress?.ToString());
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());
            diagnosticContext.Set("CorrelationId", httpContext.Items["CorrelationId"]?.ToString() ?? "unknown");
        };
    });

    // 4. Swagger (Development only)
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "OksiMin.Api v1");
            c.DisplayRequestDuration();
        });

        Log.Information("Swagger UI available at: /swagger");
        Log.Information("Seq UI available at: http://localhost:5341");
    }

    // 5. Standard ASP.NET Core middleware
    app.UseHttpsRedirection();
    app.UseCors("AllowFrontend");
    app.UseAuthorization();
    app.MapControllers();

    Log.Information("========================================");
    Log.Information("OksiMin.Api application started successfully");
    Log.Information("Environment: {Environment}", app.Environment.EnvironmentName);
    Log.Information("Middleware: CorrelationId, GlobalExceptionHandler, SerilogRequestLogging");
    Log.Information("========================================");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.Information("Shutting down OksiMin.Api application");
    Log.CloseAndFlush();
}