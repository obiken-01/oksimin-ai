using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using OksiMin.Application.Interfaces;
using OksiMin.Application.Services;
using OksiMin.Application.Validators;
using OksiMin.Infrastructure.Data;
using Serilog;
using Serilog.Events;

// ============================================
// STEP 1: Configure Serilog BEFORE building the app
// ============================================

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
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
    .WriteTo.Seq("http://localhost:5341")  // ← ADD THIS LINE
    .CreateLogger();

try
{
    Log.Information("========================================");
    Log.Information("Starting OksiMin.Api application");
    Log.Information("========================================");

    var builder = WebApplication.CreateBuilder(args);

    // ============================================
    // STEP 2: Replace default logging with Serilog
    // ============================================
    builder.Host.UseSerilog();

    // ============================================
    // STEP 3: Add services to the container
    // ============================================
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "OksiMin.Api",
            Version = "v1",
            Description = "AI-powered local information system for Occidental Mindoro"
        });
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
                  .AllowAnyMethod();
        });
    });

    Log.Information("Building application...");
    var app = builder.Build();
    Log.Information("Application built successfully");

    // ============================================
    // STEP 4: Add Serilog Request Logging
    // ============================================
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";

        // Customize log level based on response
        options.GetLevel = (httpContext, elapsed, ex) => ex != null
            ? LogEventLevel.Error
            : httpContext.Response.StatusCode > 499
                ? LogEventLevel.Error
                : httpContext.Response.StatusCode > 399
                    ? LogEventLevel.Warning
                    : LogEventLevel.Information;

        // Enrich logs with additional context
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
            diagnosticContext.Set("RemoteIP", httpContext.Connection.RemoteIpAddress?.ToString());
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());
        };
    });

    // ============================================
    // STEP 5: Configure the HTTP request pipeline
    // ============================================
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();

        Log.Information("Swagger UI available at: {SwaggerUrl}", "/swagger");
    }

    app.UseHttpsRedirection();
    app.UseCors("AllowFrontend");
    app.UseAuthorization();
    app.MapControllers();

    Log.Information("========================================");
    Log.Information("OksiMin.Api application started successfully");
    Log.Information("Environment: {Environment}", app.Environment.EnvironmentName);
    Log.Information("========================================");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "========================================");
    Log.Fatal(ex, "Application terminated unexpectedly");
    Log.Fatal(ex, "========================================");
}
finally
{
    Log.Information("Shutting down OksiMin.Api application");
    Log.CloseAndFlush();
}