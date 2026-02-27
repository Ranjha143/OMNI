using Omni_Courier_Service;

using Omni_Courier_Service.Watchers;

using Microsoft.Extensions.Hosting.Systemd;
using Microsoft.Extensions.Hosting.WindowsServices;
using MongoDB.Driver;
using Quartz;
using Serilog;


var builder = Host.CreateApplicationBuilder(args);


//builder.Services.AddSingleton<InventoryService>();



#region //============================================== Start Logging Configurations

// Build a Serilog configuration
var loggerConfig = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("ServiceName", "Courier Service")
    .WriteTo.Console() // Always log to console
    .WriteTo.MongoDB(
        builder.Configuration["MongoDB:loggingConnectionString"] ?? "", // uses same connection string
        collectionName: "Courier_service_Log"
    );

// Platform-specific sinks
if (WindowsServiceHelpers.IsWindowsService())
{
    builder.Services.AddWindowsService(options =>
    {
        options.ServiceName = "Courier Service";
    });

    // Add EventLog sink
    loggerConfig.WriteTo.EventLog("Courier Service");
}
else if (SystemdHelpers.IsSystemdService())
{
    builder.Services.AddSystemd();
    // Journal typically just captures stdout, so console is enough
}

// Create and register logger
Log.Logger = loggerConfig.CreateLogger();
builder.Logging.ClearProviders();           // remove default providers
builder.Logging.AddSerilog(Log.Logger);      // plug Serilog into host

//============================================== END Logging Configurations
#endregion

// Worker (only if you still need it)
builder.Services.AddHostedService<Worker>();

#region //============================================== Start MongoDB Configurations
builder.Services.AddOptions<MongoDbSettings>()
    .Bind(builder.Configuration.GetSection("MongoDB"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var logger = sp.GetRequiredService<ILogger<Program>>();

    var connectionString = configuration["MongoDB:ConnectionString"];
    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("MongoDB connection string is not configured.");
    }

    var settings = MongoClientSettings.FromConnectionString(connectionString);
    settings.RetryWrites = true;
    settings.RetryReads = true;
    settings.ConnectTimeout = TimeSpan.FromSeconds(30);
    settings.ServerSelectionTimeout = TimeSpan.FromSeconds(30);

    var client = new MongoClient(settings);
    try
    {
        client.ListDatabaseNames(); // test connection at startup
        return client;
    }
    catch (MongoException ex)
    {
        logger.LogError(ex, "Failed to connect to MongoDB during startup.");
        throw;
    }
});
//============================================== END MongoDB Configurations
#endregion


#region //============================================== Start Quartz (empty, waiting for dynamic jobs) Configurations
builder.Services.AddQuartz();
builder.Services.AddQuartzHostedService(options =>
{
    options.WaitForJobsToComplete = true;
});
//============================================== END Quartz (empty, waiting for dynamic jobs) Configurations
#endregion

builder.Services.AddHostedService<CourierAssignment>();
builder.Services.AddHostedService<CourierConfigurationWatcher>();

var host = builder.Build();
host.Run();