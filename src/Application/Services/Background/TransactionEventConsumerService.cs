using Defender.Mongo.MessageBroker.Configuration;
using Defender.Mongo.MessageBroker.Interfaces.Queue;
using Defender.WalletService.Application.Common.Interfaces.Services;
using Defender.WalletService.Application.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Defender.WalletService.Application.Services.Background;

public class TransactionEventConsumerService(
    IServiceScopeFactory serviceScopeFactory,
    ITransactionProcessingService transactionProcessingService)
    : BackgroundService, IDisposable
{
    private readonly SubscribeOptions<NewTransactionCreatedEvent> _subscribeOption =
        SubscribeOptionsBuilder<NewTransactionCreatedEvent>.Create()
            .SetAction(transactionProcessingService.ProcessTransaction)
            .Build();

    private const int RunEachMinute = 1;
    private Timer? _timer;
    private bool _isRunning = false;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _timer = new Timer(async _ => await Retry(null, stoppingToken),
            null, TimeSpan.FromMinutes(RunEachMinute), TimeSpan.FromMinutes(RunEachMinute));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = serviceScopeFactory.CreateScope();

                var consumer = scope.ServiceProvider.GetRequiredService<IQueueConsumer<NewTransactionCreatedEvent>>();

                await consumer.SubscribeQueueAsync(
                    _subscribeOption,
                    stoppingToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                await Task.Delay(10000, stoppingToken);
            }
        }
    }

    private async Task Retry(object? state, CancellationToken stoppingToken)
    {
        if (_isRunning)
        {
            return;
        }

        _isRunning = true;
        try
        {
            using var scope = serviceScopeFactory.CreateScope();

            var consumer = scope.ServiceProvider.GetRequiredService<IQueueConsumer<NewTransactionCreatedEvent>>();

            await consumer.RetryMissedEventsAsync(
                _subscribeOption,
                stoppingToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            _isRunning = false;
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _timer?.Dispose();
        base.Dispose();
    }
}
