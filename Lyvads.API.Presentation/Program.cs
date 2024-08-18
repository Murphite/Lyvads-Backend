using Lyvads.API.Extensions;
using Lyvads.API.Presentation.Middlewares;
using Lyvads.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDbServices(builder.Configuration);
builder.Services.AddServices(builder.Configuration);
builder.Services.AddSwaggerGen();
builder.Logging.AddSerilog();
builder.Host.UseSerilog((context, configuration) => configuration.ReadFrom.Configuration(context.Configuration));

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseSwagger();
app.UseSwaggerUI();


app.UseHttpsRedirection();
app.UseStaticFiles();

var webSocketOptions = new WebSocketOptions()
{
    KeepAliveInterval = TimeSpan.FromMinutes(2),
    ReceiveBufferSize = 4 * 1024
};

using (var servicescope = app.Services.CreateScope())
{
    var services = servicescope.ServiceProvider;
    var _context = services.GetRequiredService<AppDbContext>();

    _context.Database.Migrate();
}

app.UseWebSockets(webSocketOptions);
//app.UseMiddleware<WebSocketMiddleware>();
app.UseMiddleware<ExceptionMiddleware>();
app.UseCors("AllowAllOrigins");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
