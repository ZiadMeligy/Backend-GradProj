using GP_Server.Domain.Entities;
using GP_Server.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using GP_Server.Application.Services;
using GP_Server.Domain.Interfaces;
using GP_Server.Infrastructure.Repositories;
using GP_Server.Application.Middlewares;
using GP_Server.Application.Dep_Inj;
using Microsoft.OpenApi.Models;
using GP_Server.Infrastructure.Seeders;
using GP_Server.Application.Interfaces;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using GP_Server.Application.BackgroundServices;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers().AddJsonOptions(options =>
{
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo { Title = "WEB API", Version = "v1" });
    opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });

    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});

builder.Services.AddSingleton(new TokenService(
    builder.Configuration["Jwt:SecretKey"],
    builder.Configuration["Jwt:Issuer"],
    builder.Configuration.GetValue<int>("Jwt:ExpirationInHours")
));

#region Application Services Dependency Injection
builder.Services.AddApplicationServices(builder.Configuration);
#endregion

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IFileService, FileService>();

builder.Services.AddHttpClient();
#region Database Context Dependency Injection
var LocalPostgreSQLConnectionString = builder.Configuration.GetConnectionString("PostgresConnection") ;
var DefaultConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// PostgreSQL configuration with retry policy
builder.Services.AddDbContext<ApplicationDbContext>(options => 
    options.UseNpgsql(LocalPostgreSQLConnectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorCodesToAdd: null);
    }));
#endregion

#region Identity Configuration
builder.Services.AddIdentity<ApplicationUser, Role>(options => options.User.RequireUniqueEmail = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
#endregion

#region Jwt Configuration
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!))
    };
});
#endregion

builder.Services.AddCors(options =>
{
    // Optional: Add another policy for development
    options.AddPolicy("Development",
        policy =>
        {
            policy
                .WithOrigins(
                    "http://localhost:8000",
                    "http://localhost:8080",
                    "http://localhost:5173")
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
});

builder.Services.AddHostedService<ReportGenerationWorker>();

var app = builder.Build();

#region Seed Data
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
}
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();

    // Create database if it doesn't exist
    context.Database.EnsureCreated();

    try
    {
        await RoleSeeder.SeedRolesAsync(services);
        await UserSeeder.SeedUsersAsync(services);
    }
    catch (Exception)
    {
        throw;
    }
}

#endregion

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("Development");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapControllers();
app.Run();


