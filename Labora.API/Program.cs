using Labora.Application.Services;
using Labora.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FluentValidation;
using Labora.Application.Interfaces;
using Labora.Infrastructure.Repositories;
using Labora.Domain.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAdminPanel", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(swaggerGenOptions =>
{
    swaggerGenOptions.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT token kiriting: Bearer {token}"
    });
    swaggerGenOptions.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// Application
builder.Services.AddApplication(builder.Configuration);

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<Labora.Application.Validators.Auth.RegisterRequestDtoValidator>();

// Infrastructure
builder.Services.AddInfrastructure(builder.Configuration);

// Push Notifications
builder.Services.AddHttpClient("ExpoPush", client =>
{
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
});
builder.Services.AddScoped<Labora.Domain.Interfaces.IPushTokenRepository,
    Labora.Infrastructure.Repositories.PushTokenRepository>();
builder.Services.AddScoped<Labora.Application.Interfaces.IPushNotificationService,
    Labora.Application.Services.PushNotificationService>();

// Notification
builder.Services.AddScoped<Labora.Domain.Interfaces.INotificationRepository,
    Labora.Infrastructure.Repositories.NotificationRepository>();
builder.Services.AddScoped<Labora.Domain.Interfaces.IUserPreferenceRepository,
    Labora.Infrastructure.Repositories.UserPreferenceRepository>();
builder.Services.AddScoped<Labora.Application.Interfaces.INotificationService,
    Labora.Application.Services.NotificationService>();

// WorkerPost
builder.Services.AddScoped<IWorkerPostRepository, WorkerPostRepository>();
builder.Services.AddScoped<IWorkerPostService, WorkerPostService>();
builder.Services.AddScoped<ISavedJobRepository, SavedJobRepository>();
builder.Services.AddScoped<ISavedJobService, SavedJobService>();

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

var app = builder.Build();

// Render terminates TLS and proxies to this container over a single hop.
// ForwardLimit = 1 with cleared KnownNetworks/KnownProxies trusts only the
// immediate (and only) proxy hop, so a client cannot spoof RemoteIpAddress
// by prepending forged entries to X-Forwarded-For.
// TODO: verify against Render's current proxy topology before relying on this
// for abuse-protection decisions (see deployment questions in task report).
var forwardedHeadersOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    ForwardLimit = 1,
};
forwardedHeadersOptions.KnownNetworks.Clear();
forwardedHeadersOptions.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedHeadersOptions);

app.UseCors("AllowAdminPanel");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<Labora.API.Middleware.ExceptionHandlingMiddleware>();
app.UseStaticFiles();
//app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();