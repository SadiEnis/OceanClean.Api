using OceanClean.Api.Data;
using OceanClean.Api.Repositories;
using OceanClean.Api.Services;
using OceanClean.Api.Security;

using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OceanClean.Api.Services.Admin;
using OceanClean.Api.Repositories.Admin;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AdminFrontend", policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT access token."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("Jwt")
);

builder.Services.AddSingleton<JwtTokenService>();
builder.Services.AddSingleton<RefreshTokenService>();

var jwtSettings = builder.Configuration
    .GetSection("Jwt")
    .Get<JwtSettings>();

if (jwtSettings == null || string.IsNullOrWhiteSpace(jwtSettings.SecretKey))
{
    throw new InvalidOperationException("JWT settings are missing.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,

            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.SecretKey)
            ),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("super_admin", "admin", "moderator", "viewer"));

    options.AddPolicy("ModeratorOrAbove", policy =>
        policy.RequireRole("super_admin", "admin", "moderator"));

    options.AddPolicy("AdminOrAbove", policy =>
        policy.RequireRole("super_admin", "admin"));

    options.AddPolicy("SuperAdminOnly", policy =>
        policy.RequireRole("super_admin"));
});

builder.Services.AddSingleton<MySqlConnectionFactory>();

builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<AuthService>();

builder.Services.AddScoped<PlayerProfileRepository>();
builder.Services.AddScoped<PlayerProfileService>();

builder.Services.AddScoped<MatchRepository>();
builder.Services.AddScoped<MatchService>();

builder.Services.AddScoped<ShopRepository>();
builder.Services.AddScoped<ShopService>();

builder.Services.AddScoped<InventoryRepository>();
builder.Services.AddScoped<InventoryService>();

builder.Services.AddScoped<AdminAuthRepository>();
builder.Services.AddScoped<AdminAuthService>();
builder.Services.AddScoped<AdminPlayersRepository>();
builder.Services.AddScoped<AdminPlayersService>();
builder.Services.AddScoped<AdminMatchesRepository>();
builder.Services.AddScoped<AdminMatchesService>();
builder.Services.AddScoped<AdminEconomyRepository>();
builder.Services.AddScoped<AdminEconomyService>();
builder.Services.AddScoped<AdminEventsRepository>();
builder.Services.AddScoped<AdminEventsService>();

builder.Services.AddScoped<AdminDashboardRepository>();
builder.Services.AddScoped<AdminDashboardService>();

builder.Services.AddScoped<AdminAuditLogsRepository>();
builder.Services.AddScoped<AdminAuditLogsService>();

var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

var swaggerEnabled = builder.Configuration.GetValue<bool>("Swagger:Enabled");

if (app.Environment.IsDevelopment() || swaggerEnabled)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors("AdminFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers().RequireCors("AdminFrontend");

app.Run();