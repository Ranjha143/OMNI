using Omni_Courier_Service.Services;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omni_Courier_Service.Jobs
{
    [DisallowConcurrentExecution]
    public class InventoryJob : IJob
    {
        private readonly InventoryService _service;
        private readonly ILogger<InventoryJob> _logger;

        public InventoryJob(InventoryService processor, ILogger<InventoryJob> logger)
        {
            _service = processor;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("InventoryJob triggered at {time}", DateTimeOffset.Now);
            await _service.RunAsync();
        }
    }
}
