using Lyvads.API.Extensions;
using Lyvads.API.Presentation.Extensions;
using Lyvads.API.Presentation.Middlewares;
using Lyvads.Domain.Constants;
using Lyvads.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDbServices(builder.Configuration);
builder.Services.AddServices(builder.Configuration);
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Lyvads API", Version = "v1" });

    c.OperationFilter<RemoveDefaultResponseFilter>();
    c.OperationFilter<SwaggerFileUploadOperationFilter>();
});

builder.Logging.AddSerilog();
builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration.ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File("./logs/log-.txt", rollingInterval: RollingInterval.Day);
});

var app = builder.Build();

// Run database migrations automatically on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        dbContext.Database.Migrate();
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred while migrating the database.");
        throw;
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();  // Detailed error pages for development
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();

// Custom middleware for exception handling
app.UseMiddleware<ExceptionMiddleware>();

// Enable CORS
app.UseCors();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
