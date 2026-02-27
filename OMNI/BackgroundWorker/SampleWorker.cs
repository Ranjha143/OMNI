

namespace OMNI.BackgroundWorker
{
    internal class SampleWorker(ILogger<Worker> logger) : BackgroundService
    {
        private readonly ILogger<Worker> _logger = logger;
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            int delay = 36000;

            while (!stoppingToken.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation(" running at: {time}", DateTimeOffset.Now);
                }
                await Task.Delay(delay, stoppingToken);
            }
        }
    }
}
