using Radzen;
using BTSS.Rating.Components;
using BTSS.Rating.Application;
using BTSS.Rating.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
      .AddInteractiveServerComponents().AddHubOptions(options => options.MaximumReceiveMessageSize = 10 * 1024 * 1024);

builder.Services.AddControllers();
builder.Services.AddRadzenComponents();

builder.Services.AddRadzenCookieThemeService(options =>
{
    options.Name = "BTSS.Rating.AdminTheme";
    options.Duration = TimeSpan.FromDays(365);
});
builder.Services.AddHttpClient();
builder.Services.AddHttpClient("RatingApi", (sp, client) =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    var baseUrl = cfg["RatingApi:BaseUrl"] ?? "https://localhost:5001/";
    client.BaseAddress = new Uri(baseUrl);
});
builder.Services.AddScoped<BTSS.Rating.Admin.Services.RatingApiClient>();
builder.Services.AddRatingApplication(builder.Configuration);
builder.Services.AddRatingInfrastructure(builder.Configuration);
var app = builder.Build();

var forwardingOptions = new ForwardedHeadersOptions()
{
    ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
};
forwardingOptions.KnownIPNetworks.Clear();
forwardingOptions.KnownProxies.Clear();

app.UseForwardedHeaders(forwardingOptions);


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found");
app.UseHttpsRedirection();
app.MapControllers();
app.MapStaticAssets();
app.UseAntiforgery();
app.UseRouting();
app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode();

app.Run();