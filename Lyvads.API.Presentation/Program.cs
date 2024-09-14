using Lyvads.API.Extensions;
using Lyvads.API.Presentation.Middlewares;
using Lyvads.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDbServices(builder.Configuration);
builder.Services.AddServices(builder.Configuration);
builder.Services.AddSwaggerGen();

// Add Serilog for logging
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
app.UseRouting();
app.UseAuthorization();

using (var servicescope = app.Services.CreateScope())
{
    var services = servicescope.ServiceProvider;
    var _context = services.GetRequiredService<AppDbContext>();
    _context.Database.Migrate();
}

app.UseMiddleware<ExceptionMiddleware>();
app.UseCors("AllowAllOrigins");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
