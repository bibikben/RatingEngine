using System.Reflection;
using BTSS.Rating.Application;
using BTSS.Rating.Infrastructure;
using Microsoft.OpenApi;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BTSS Rating API",
        Version = "v1",
        Description = "Shipment rating interface backed by RatingDb."
    });

    // Include XML comments if present
    var xmlName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlName);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath);
});

builder.Services.AddRatingApplication(builder.Configuration);
builder.Services.AddRatingInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "BTSS Rating API v1");
        c.DocumentTitle = "BTSS Rating API - Swagger";
    });
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
