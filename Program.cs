using Microsoft.IdentityModel.Tokens;
using System.Text;
using OIT_Reservation.Interface;
using OIT_Reservation.Services;
using OIT_Reservation.Helpers;
using Serilog;
using Serilog.Exceptions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using YourNamespace.Services; // For JwtHelper

var builder = WebApplication.CreateBuilder(args);

// Get log file paths from config
var logFilePath = builder.Configuration["Logging:LogFilePath"];
var errorLogFilePath = builder.Configuration["Logging:ErrorLogFilePath"];

// Provide default paths if config values are missing
if (string.IsNullOrWhiteSpace(logFilePath))
    logFilePath = "Logs/log.txt";
if (string.IsNullOrWhiteSpace(errorLogFilePath))
    errorLogFilePath = "Logs/error.txt";

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithExceptionDetails()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File(
        path: logFilePath,
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
    )
    .WriteTo.File(
        path: errorLogFilePath,
        restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error,
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
    )
    .CreateLogger();
 // ✅ IMPORTANT - routes ILogger<T> to Serilog
 // ✅ MUST be here
builder.Host.UseSerilog();
// Add Serilog to the logging pipeline
// Add CORS policy (Allow all for testing)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add controllers
builder.Services.AddControllers();

// Add logging
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.AddDebug();
    logging.AddEventLog(settings =>
    {
        settings.LogName = "Application";
        settings.SourceName = "MyAppName";
    });
});

// Swagger with JWT support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "OIT Reservation API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your token"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
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
            new string[]{}
        }
    });
});

// Register your app services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<JwtHelper>(); // <-- Register your JwtHelper here
builder.Services.AddScoped<RoomTypeService>();
builder.Services.AddScoped<EventTypeService>();
builder.Services.AddScoped<TravelAgentService>();
builder.Services.AddScoped<setupStyleService>();
builder.Services.AddScoped<PackageInfoService>();
builder.Services.AddScoped<ServiceTypeService>();
builder.Services.AddScoped<CustomerService>();
builder.Services.AddScoped<CountryService>();
builder.Services.AddScoped<CustomerTypeService>();
builder.Services.AddScoped<TitleService>();
builder.Services.AddScoped<NationalityService>();
builder.Services.AddScoped<RoomService>();
builder.Services.AddScoped<BookingResourceService>();
builder.Services.AddScoped<IPayTypeService, PayTypeService>();
builder.Services.AddScoped<IRoomReservationService, RoomReservationService>();
builder.Services.AddScoped<IReservationStatusService, ReservationStatusService>();
builder.Services.AddScoped<IReservationCalendarService, ReservationCalendarService>();

// Register CustomerService


// JWT Authentication setup - this only validates incoming tokens
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]);

builder.Services.AddAuthentication(options =>
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
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "KOT v1");
    c.DefaultModelExpandDepth(-1);
    c.EnableFilter();
});
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("swagger/v1/swagger.json", "KOT v1"); // Adjust the relative path
    c.RoutePrefix = ""; // Optional: Serve Swagger at the root 
});

if (app.Environment.IsProduction())
{
    app.UseHsts();
    app.UseExceptionHandler("/error");
}

if (app.Environment.IsProduction())
{
    app.UseHsts();
    app.UseExceptionHandler("/error");
}



// Middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

app.UseHsts();
app.UseExceptionHandler("/error");

app.UseMiddleware<RequestResponseLoggingMiddleware>();

app.UseRouting();

app.UseCors("AllowAll");

app.UseAuthentication(); // Validate JWT on incoming requests
app.UseAuthorization(); // Authorize incoming requests
app.MapControllers();


app.Run();
