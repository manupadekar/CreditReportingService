using CreditReporting.Application.Interfaces;
using CreditReporting.Application.Mappings;
using CreditReporting.Application.Services;
using CreditReporting.Infrastructure.Data;
using CreditReporting.Infrastructure.ExternalServices;
using CreditReportingService.Middleware;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;
using Serilog;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// 1. Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/credit-reporting-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// 2. Add services to the container.

// Add Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("fixed", opt =>
    {
        opt.Window = TimeSpan.FromSeconds(10);
        opt.PermitLimit = 10; // Higher limit for credit service
        opt.QueueLimit = 0;
    });
});

builder.Services.AddHttpClient<ICustomerServiceClient, CustomerServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ExternalServices:CustomerApi"]);
});

// Database Connection
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

// Dependency Injection
builder.Services.AddScoped<ICibilService, CibilService>();


// AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 3. Use Custom Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// 4. Use Rate Limiting
app.UseRateLimiter();

app.UseAuthorization();
app.MapControllers();

app.Run();
