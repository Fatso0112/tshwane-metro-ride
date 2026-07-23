using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using TshwaneMetroRide.Api.Data;
using TshwaneMetroRide.Api.Interfaces;
using TshwaneMetroRide.Api.Models;
using TshwaneMetroRide.Api.Services;
using TshwaneMetroRide.Api.Options;
using TshwaneMetroRide.Api.Services.Email;
using TshwaneMetroRide.Api.Services.Otp;

var builder = WebApplication.CreateBuilder(args);

var connectionString =
    builder.Configuration.GetConnectionString(
        "DefaultConnection")
    ?? throw new InvalidOperationException(
        "DefaultConnection was not found.");

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException(
        "The JWT signing key is missing.");

var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException(
        "The JWT issuer is missing.");

var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException(
        "The JWT audience is missing.");

builder.Services.AddDbContext<ApplicationDbContext>(
    options =>
        options.UseMySql(
            connectionString,
            new MariaDbServerVersion(
                new Version(10, 4, 32))));

builder.Services.AddScoped<
    IPasswordHasher<Passenger>,
    PasswordHasher<Passenger>>();

builder.Services.AddScoped<
    ITokenServices,
    TokenService>();

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtKey)),

                ClockSkew = TimeSpan.Zero
            };
    });

builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description =
                "Paste the JWT access token."
        });

    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type =
                            ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
});

builder.Services.Configure<SmtpOptions>(
    builder.Configuration.GetSection(
        SmtpOptions.SectionName));

builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.Configure<OtpOptions>(
    builder.Configuration.GetSection(
        OtpOptions.SectionName));

builder.Services.AddScoped<
    IEmailOtpService,
    EmailOtpService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();