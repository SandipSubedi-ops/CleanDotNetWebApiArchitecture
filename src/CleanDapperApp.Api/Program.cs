using CleanDapperApp.Api.Extensions;
using CleanDapperApp.Api.Middleware;
using Hangfire;
using Hangfire.MemoryStorage;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog Configuration
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.ConfigureServices(builder.Configuration);
builder.Services.ConfigureJwt(builder.Configuration);
builder.Services.ConfigureSwagger();

// Hangfire (Using Memory Storage for Demo)
builder.Services.AddHangfire(config => config.UseMemoryStorage());
builder.Services.AddHangfireServer();

var app = builder.Build();

// Configure Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseHangfireDashboard("/hangfire");

app.MapControllers();

// Schedule Daily Report Job automatically on startup
using (var scope = app.Services.CreateScope())
{
    // var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
    // recurringJobManager.AddOrUpdate<cleanDapperApp.Models.Interfaces.IEmailService>(
    //     "daily-report-job",
    //     x => x.SendDailyReportsAsync(),
    //     Cron.Daily(12, 0));
}

app.Run();
