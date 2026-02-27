

using Shopify;
using RetailPro2_X;

namespace OMNI
{
    public class Worker(IConfiguration configuration, ILogger<Worker> logger) : BackgroundService
    {
        private readonly ILogger<Worker> _logger = logger;
        private readonly IConfiguration _configuration = configuration;
        private readonly ShopifyPlugin shopifyPlugin = new ShopifyPlugin();
        private readonly RetailProV22_Plugin retailProV22_Plugin = new RetailProV22_Plugin();

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            retailProV22_Plugin.Start();
            Task.Delay(10000).Wait();
            shopifyPlugin.Start();

            while (!stoppingToken.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("OMNI running, time now is:  {time}", DateTimeOffset.Now);
                }
                await Task.Delay(36000, stoppingToken);
            }
        }
    }

}
