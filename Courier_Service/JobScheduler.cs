using MongoDB.Driver;
using Quartz;
using System.Threading;
using System.Threading.Tasks;

public class JobScheduler : BackgroundService
{
    private readonly IMongoCollection<JobDefinition> _jobCollection;
    private readonly ISchedulerFactory _schedulerFactory;

    public JobScheduler(IMongoClient client, ISchedulerFactory schedulerFactory, IConfiguration config)
    {
        var databaseName = config["MongoDB:DataBase"];
        var db = client.GetDatabase(databaseName);
        _jobCollection = db.GetCollection<JobDefinition>("JobSchedules");
        _schedulerFactory = schedulerFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var scheduler = await _schedulerFactory.GetScheduler(stoppingToken);

        // Run all jobs once at startup
        var filter = Builders<JobDefinition>.Filter.And(
            Builders<JobDefinition>.Filter.Eq(j => j.isActive, true),
            Builders<JobDefinition>.Filter.Eq(j => j.jobCategory, "RetailPro_Inventory")
        );

        var allJobs = await _jobCollection.Find(filter).ToListAsync(stoppingToken);
        foreach (var job in allJobs)
            await ScheduleOrUpdateJob(job, stoppingToken);

        // Watch for future changes
        using var changeStream = _jobCollection.Watch(cancellationToken: stoppingToken);

        await foreach (var change in changeStream.ToAsyncEnumerable().WithCancellation(stoppingToken))
        {
            var doc = change.FullDocument;
            await ScheduleOrUpdateJob(doc, stoppingToken);
        }
    }

    private async Task ScheduleOrUpdateJob(JobDefinition job, CancellationToken token)
    {
        var scheduler = await _schedulerFactory.GetScheduler(token);
        var jobKey = new JobKey(job.id);

        if (await scheduler.CheckExists(jobKey, token))
        {
            await scheduler.DeleteJob(jobKey, token);
        }


        // Try to resolve the job type reliably
        var typeName = job.jobType.Split(',')[0].Trim();
        var type = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .FirstOrDefault(t => t.FullName == typeName);

        if (type == null)
            throw new Exception($"Could not resolve job type: {job.jobType}");



        var jobDetail = JobBuilder.Create(type)
            .WithIdentity(job.id)
            .Build();

        var trigger = TriggerBuilder.Create()
            .WithIdentity($"{job.id}-trigger")
            .StartNow()
            .WithCronSchedule(job.cronExpression)
            .Build();

        await scheduler.ScheduleJob(jobDetail, trigger, token);
    }

    public class JobDefinition
    {
        public string id { get; set; } = default!;
        public string jobType { get; set; } = default!;
        public string cronExpression { get; set; } = default!;
        public bool isActive { get; set; } = true;
        public string jobCategory { get; set; } = default!;
    }


}
