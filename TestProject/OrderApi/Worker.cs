using OrderApi.DataLayer.Interfaces;

namespace OrderApi;

public class Worker : BackgroundService
{
    #region DI

    private readonly ILogger<Worker> _logger;
    private readonly IOrderQueue _orderQueue;
    private readonly IOrderAggregator _orderAggregator;

    #endregion

    private DateTime _lastPublishTime = DateTime.MinValue;
    private readonly int _publishingPeriod;
    private readonly int _orderCountTrigger;

    public Worker(ILogger<Worker> logger, IOrderQueue orderQueue, IOrderAggregator orderAggregator, IConfiguration configuration)
    {
        _logger = logger;
        _orderQueue = orderQueue;
        _orderAggregator = orderAggregator;

        _publishingPeriod = configuration.GetValue<int>("Publishing:PublishingPeriod");
        _orderCountTrigger = configuration.GetValue<int>("Publishing:OrderCountTrigger");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await OneIteration(stoppingToken);
        }
    }

    public async Task<int> OneIteration(CancellationToken stoppingToken)
    {
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
        }

        // publikuj objednavky pokud...
        if ((DateTime.UtcNow - _lastPublishTime).TotalSeconds > _publishingPeriod ||   // ubehl stanoveny cas od posledniho publikovani
            _orderQueue.ItemCount() >= _orderCountTrigger)                             // pocet obejdnavek dosahl (ci presahl) limit
        {
            var res = _orderAggregator.AggregateOrders();
            _lastPublishTime = DateTime.UtcNow;
            return res.Count;
        }

        await Task.Delay(1000, stoppingToken).ContinueWith(ContinueCancelledTask, stoppingToken);
        return 0;
    }

    private void ContinueCancelledTask(Task tsk)
    {
        if (tsk.IsCanceled)
            _logger.LogInformation("Cancelled due to a StoppingToken");
    }
}