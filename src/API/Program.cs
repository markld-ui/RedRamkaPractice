using API.Middleware;
using API.Services;
using Application.Common.Interfaces;
using Application.Features.Projects.Commands;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Persistence;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var services = builder.Services;

var applicationAssembly = typeof(CreateProjectCommand).Assembly;

// ──────────────────────────────────────────────
// MVC
// ──────────────────────────────────────────────
services.AddControllers()
    .AddJsonOptions(opts =>
        opts.JsonSerializerOptions.PropertyNamingPolicy = null);

services.AddEndpointsApiExplorer();

// ──────────────────────────────────────────────
// Swagger
// ──────────────────────────────────────────────
services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Project Lifecycle Service API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter the JWT token: Bearer {your_token}"
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
            new List<string>()
        }
    });
});

// ──────────────────────────────────────────────
// CORS
// ──────────────────────────────────────────────
services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod()));

// ──────────────────────────────────────────────
// Database
// ──────────────────────────────────────────────
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

services.AddScoped<IApplicationDbContext, ApplicationDbContext>();
services.AddScoped<ApplicationDbContextSeed>();

// ──────────────────────────────────────────────
// MediatR & Validation
// ──────────────────────────────────────────────
services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(applicationAssembly));

services.AddValidatorsFromAssembly(applicationAssembly);

// ──────────────────────────────────────────────
// Application Services
// ──────────────────────────────────────────────
services.AddHttpContextAccessor();
services.AddScoped<ICurrentUserService, CurrentUserService>();
services.AddScoped<IDateTime, DateTimeService>();
services.AddScoped<IPasswordHasher, PasswordHasher>();
services.AddScoped<ITokenService, TokenService>();
services.AddScoped<IProjectAuthorizationService, ProjectAuthorizationService>();

// ──────────────────────────────────────────────
// Authentication & Authorization
// ──────────────────────────────────────────────
var jwtSettings = configuration.GetSection("Jwt");

services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.ASCII.GetBytes(jwtSettings["Key"]))
    };
});

services.AddAuthorization();

// ──────────────────────────────────────────────
// Pipeline
// ──────────────────────────────────────────────
var app = builder.Build();

app.UseMiddleware<GlobalExceptionHandler>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// ──────────────────────────────────────────────
// Database Migrations & Seeding
// ──────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

using (var scope = app.Services.CreateScope())
{
    var seed = scope.ServiceProvider.GetRequiredService<ApplicationDbContextSeed>();
    await seed.SeedAsync();
}

app.Run();