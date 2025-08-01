using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RailwayApp.Application.Services;
using RailwayApp.Application.Services.AdminServices;
using RailwayApp.Application.Services.PasswordHashers;
using RailwayApp.Domain.Interfaces.IServices;
using RailwayApp.Domain.Interfaces.IServices.AdminServices;
using RailwayApp.Infrastructure;
using RailwayApp.Web.Middlewares;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", false);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        "logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7,
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}"
    )
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();
builder.Services.AddScoped<ICarriageAvailabilityUpdateService, CarriageAvailabilityUpdateService>();
builder.Services.AddScoped<ICarriageSeatService, CarriageSeatService>();
builder.Services.AddScoped<ICarriageService, CarriageService>();
builder.Services.AddScoped<ICarriageTemplateService, CarriageTemplateService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IPriceCalculationService, PriceCalculationService>();
builder.Services.AddScoped<IRouteSearchService, RouteSearchService>();
builder.Services.AddScoped<IScheduleService, ScheduleService>();
builder.Services.AddScoped<IStationService, StationService>();
builder.Services.AddScoped<ITicketBookingService, TicketBookingService>();
builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddScoped<IUserAccountService, UserAccountService>();

builder.Services.AddScoped<IAdminAbstractRouteService, AdminAbstractRouteService>();
builder.Services.AddScoped<IAdminAbstractRouteSegmentService, AdminAbstractRouteSegmentService>();
builder.Services.AddScoped<IAdminCarriageAvailabilityService, AdminCarriageAvailabilityService>();
builder.Services.AddScoped<IAdminCarriageTemplateService, AdminCarriageTemplateService>();
builder.Services.AddScoped<IAdminConcreteRouteService, AdminConcreteRouteService>();
builder.Services.AddScoped<IAdminConcreteRouteSegmentService, AdminConcreteRouteSegmentService>();
builder.Services.AddScoped<IAdminSeatLockService, AdminSeatLockService>();
builder.Services.AddScoped<IAdminStationService, AdminStationService>();
builder.Services.AddScoped<IAdminTicketService, AdminTicketService>();
builder.Services.AddScoped<IAdminTrainService, AdminTrainService>();
builder.Services.AddScoped<IAdminTrainTypeService, AdminTrainTypeService>();
builder.Services.AddScoped<IAdminUserAccountService, AdminUserAccountService>();

builder.Services.AddMongoDb(builder.Configuration);

var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddHostedService<ConcreteRouteGeneratorService>();
builder.Services.AddHostedService<TicketUpdateStatusService>();
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "RailwayApp API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"JWT Authorization header using the Bearer scheme. 
                          Enter 'Bearer' [space] and then your token in the text input below.
                          Example: 'Bearer 12345abcdef'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactClient", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "https://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "RailwayApp API V1");
        c.EnablePersistAuthorization();
    });
}

app.UseHttpsRedirection();
app.UseCors("ReactClient");
app.UseCustomExceptionHandling();
app.UseAuthentication();
app.UseUserSessionValidator();
app.UseAuthorization();
app.MapControllers();
app.Run();