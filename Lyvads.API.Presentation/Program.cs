using Lyvads.API.Extensions;
using Lyvads.API.Presentation.Extensions;
using Lyvads.API.Presentation.Middlewares;
using Lyvads.Domain.Constants;
using Lyvads.Infrastructure.Persistence;
using Lyvads.Infrastructure.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using Stripe;
using System.Text.Json.Serialization;


var builder = WebApplication.CreateBuilder(args);

// Configure services
builder.Services.AddControllers()
    .AddNewtonsoftJson()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDbServices(builder.Configuration);
builder.Services.AddServices(builder.Configuration);
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Lyvads API", Version = "v1" });
    c.OperationFilter<RemoveDefaultResponseFilter>();
    c.OperationFilter<SwaggerFileUploadOperationFilter>();
});

builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));
StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

// Logging
builder.Logging.AddSerilog();

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration.ReadFrom.Configuration(context.Configuration)
                 .ReadFrom.Services(services)
                 .Enrich.FromLogContext()
                 .WriteTo.Console()
                 .WriteTo.File("./logs/log-.txt", rollingInterval: RollingInterval.Day);
});

builder.Services.AddScoped<DataGenerator>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins(
            "https://api.paystack.co",
            "http://localhost:3000",
            "https://lyvads-admin-dashboard.vercel.app",
            "https://radiksez.admin.lyvads.com"
        )
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

// Build app
var app = builder.Build();

// Database migration (if needed)
//using (var scope = app.Services.CreateScope())
//{
//    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
//    try
//    {
//        dbContext.Database.Migrate();
//    }
//    catch (Exception ex)
//    {
//        Log.Error(ex, "An error occurred while migrating the database.");
//        throw;
//    }
//}

// Middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseCors("AllowSpecificOrigins");
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();
app.UseMiddleware<ExceptionMiddleware>();

app.MapControllers();

//await Seeder.Run(app);

app.Run();
