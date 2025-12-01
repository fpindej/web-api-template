using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.OpenApi.Models;
using MyProject.Infrastructure.Features.Authentication.Extensions;
using MyProject.Infrastructure.Features.Postgres.Extensions;
using MyProject.WebApi.Extensions;
using MyProject.WebApi.Middlewares;
using Scalar.AspNetCore;
using Serilog;
using LoggerConfigurationExtensions = MyProject.Infrastructure.Logging.Extensions.LoggerConfigurationExtensions;

var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? Environments.Production;

Log.Logger = LoggerConfigurationExtensions.ConfigureMinimalLogging(environmentName);

try
{
    Log.Information("Starting web host");
    var builder = WebApplication.CreateBuilder(args);

    Log.Debug("Use Serilog");
    builder.Host.UseSerilog((context, _, loggerConfiguration) =>
    {
        LoggerConfigurationExtensions.SetupLogger(context.Configuration, loggerConfiguration);
    }, true);

    try
    {
        Log.Debug("Adding TimeProvider");
        builder.Services.AddSingleton(TimeProvider.System);

        Log.Debug("Adding persistence services");
        builder.Services.AddPersistence(builder.Configuration);

        Log.Debug("Adding identity services");
        builder.Services.AddIdentityServices(builder.Configuration);
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, "Failed to configure essential services or dependencies.");
        throw;
    }

    Log.Debug("Adding Controllers");
    builder.Services.AddControllers();

    Log.Debug("Adding FluentValidation");
    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddValidatorsFromAssemblyContaining<Program>();

    Log.Debug("Adding rate limiting");
    builder.Services.AddRateLimiting(builder.Configuration);

    Log.Debug("ConfigureServices => Setting AddHealthChecks");
    builder.Services.AddHealthChecks();

    if (!builder.Environment.IsProduction())
    {
        Log.Debug("ConfigureServices => Setting AddOpenApi");
        builder.Services.AddOpenApi("v1",
            opt => { opt.AddDocumentTransformer<BearerSecuritySchemeTransformer>(); });

        Log.Debug("ConfigureServices => Setting AddSwaggerGen");
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "MyProject API", Version = "v1" });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JWT Bearer token"
            });

            c.AddSecurityDefinition("Cookie", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.ApiKey,
                Name = "Cookie",
                In = ParameterLocation.Cookie,
                Description = "Authentication cookie"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, Array.Empty<string>() },
                { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Cookie" } }, Array.Empty<string>() }
            });
        });
    }

    var app = builder.Build();

    Log.Debug("Setting UseForwardedHeaders");
    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
    });

    if (app.Environment.IsProduction())
    {
        app.Use(async (context, next) =>
        {
            context.Request.Scheme = "https";
            await next();
        });
    }
    else
    {
        Log.Debug("Setting MapOpenApi");
        app.MapOpenApi();

        Log.Debug("Setting MapScalarApiReference");
        app.MapScalarApiReference(opt =>
        {
            opt.WithTitle("MyProject API");
            opt.WithTheme(ScalarTheme.Mars);
            opt.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
        });

        Log.Debug("Enabling Swagger UI");
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/openapi/v1.json", "MyProject API V1");
            c.RoutePrefix = "swagger";
            // Enable sending cookies with API requests from Swagger UI
            c.ConfigObject.AdditionalItems.Add("withCredentials", true);
        });
    }

    if (app.Environment.IsDevelopment())
    {
        Log.Debug("Setting UseDeveloperExceptionPage");
        app.UseDeveloperExceptionPage();

        Log.Debug("Apply migrations to local database");
        app.ApplyMigrations();

        Log.Debug("Seeding identity data (test and admin users)");
        await app.SeedIdentityUsersAsync();
    }

    Log.Debug("Setting cors => allow *");
    app.UseCors(b =>
    {
        b.SetIsOriginAllowed(_ => true);
        b.AllowAnyHeader();
        b.AllowAnyMethod();
        b.AllowCredentials();
    });

    Log.Debug("Setting UseSerilogRequestLogging");
    app.UseSerilogRequestLogging();

    Log.Debug("Setting UseMiddleware => ExceptionHandlingMiddleware");
    app.UseMiddleware<ExceptionHandlingMiddleware>();

    Log.Debug("Setting UseHttpsRedirection");
    app.UseHttpsRedirection();

    Log.Debug("Setting UseRateLimiter");
    app.UseRateLimiter();

    Log.Debug("Setting UseRouting");
    app.UseRouting();

    Log.Debug("Setting UseAuthentication");
    app.UseAuthentication();

    Log.Debug("Setting UseAuthorization");
    app.UseAuthorization();

    Log.Debug("Setting \"security\" measure => Redirect to YouTube video to confuse enemies");
    app.Use(async (context, next) =>
    {
        if (context.Request.Path.Value is "/")
        {
            context.Response.Redirect("https://www.youtube.com/watch?v=dQw4w9WgXcQ", permanent: false);
            return;
        }

        await next();
    });

    Log.Debug("Setting endpoints => MapControllers");
    app.MapControllers();

    Log.Debug("Setting endpoints => MapHealthChecks");
    app.MapHealthChecks("/health");

    await app.RunAsync();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.Information("Shutting down application");
    await Log.CloseAndFlushAsync();
}
