using backendDistributor.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders; // For StaticFiles
using System.IO;                         // For Path

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers().AddJsonOptions(x =>
    x.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<CustomerDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CustomerDbContext"))
        .LogTo(Console.WriteLine, LogLevel.Information) // 🔥 Show SQL
           .EnableSensitiveDataLogging()
           .EnableDetailedErrors()
           );

// Define the CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy =>
        {
            policy.AllowAnyOrigin() // Allows ANY origin
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

var app = builder.Build(); // Build the application

// Configure the HTTP request pipeline.
// The order of middleware here is important.

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage(); // More detailed errors in dev
}
else
{
    app.UseExceptionHandler("/Home/Error"); // Configure a proper error handler for production
    app.UseHsts();
}

app.UseHttpsRedirection(); // Redirect HTTP to HTTPS

// E.g., wwwroot/images/products/foo.jpg will be accessible at /images/products/foo.jpg
app.UseStaticFiles();

app.UseRouting(); // Explicitly adding UseRouting (often implicitly added, but good for clarity)

// *** ADD CORS MIDDLEWARE HERE ***
// It should generally be after UseRouting (implicitly added) and UseHttpsRedirection,
// and before UseAuthentication, UseAuthorization, and endpoint mapping.
app.UseCors("AllowReactApp");

app.UseAuthentication(); // If you add authentication later
app.UseAuthorization();  // Authorization middleware

app.MapControllers();    // Maps controller actions to routes

app.Run();