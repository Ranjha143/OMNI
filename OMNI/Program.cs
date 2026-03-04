using Microsoft.Extensions.Hosting.WindowsServices;
using Microsoft.Extensions.Hosting.Systemd;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using OMNI;

var builder = WebApplication.CreateBuilder(args);

var env = builder.Environment;


// Configure Services
builder.Services.AddControllers();
builder.Services.AddRazorPages();

builder.Services.AddSingleton<Globals>();

builder.Services.AddHostedService<StartupConfigLoader>();
builder.Services.AddHostedService<MongoDbInitializerHostedService>();
builder.Services.AddHostedService<Worker>();


// OS-specific hosting
if (OperatingSystem.IsWindows())
{
    builder.Host.UseWindowsService();
}
else if (OperatingSystem.IsLinux())
{
    builder.Host.UseSystemd();
}

builder.Services.AddDistributedMemoryCache(); // Required for session storage
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

if (env.IsDevelopment())
{
    // Development-specific config
}
else if (env.IsProduction())
{
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenAnyIP(4040);
        //options.ListenAnyIP(443, listenOptions =>
        //{
        //    listenOptions.UseHttps();
        //}); // or change to your preferred port
    });
}
else
{
    // Other environments, e.g., Staging
}

// CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
        //.AllowCredentials();
    });
});

builder.Services.AddAuthentication("MyCookieAuth")
    .AddCookie("MyCookieAuth", options =>
    {
        options.LoginPath = "/login";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (env.IsDevelopment())
{
    // Development-specific config
}
else if (env.IsProduction())
{
    //app.UseHttpsRedirection();
    app.UseExceptionHandler("/Error"); // Create a Razor Page or controller at /Error
    app.UseHsts(); // Adds HTTP Strict Transport Security
}
else
{
    // Other environments, e.g., Staging
}

app.UseCors("AllowAll");
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession(); // ✅ MUST be called before routing Razor Pages

// =============================================================  Map Endpoints
app.MapControllers();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();