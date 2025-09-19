using WebApplication1.Services;

namespace WebApplication1.BackgroundServices
{
    public class ItpNotificationBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ItpNotificationBackgroundService> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromMinutes(30); // Ruleaz? la fiecare 30 minute

        public ItpNotificationBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<ItpNotificationBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Serviciul Background pentru Notific?ri ITP a pornit");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var currentTime = DateTime.Now;
                    _logger.LogInformation($"Serviciul Background pentru Notific?ri ITP ruleaz? la: {currentTime}");

                    // Verific? dac? suntem în intervalul de lucru (8:00-21:00)
                    if (currentTime.Hour >= 8 && currentTime.Hour < 21)
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var notificationService = scope.ServiceProvider
                            .GetRequiredService<IItpNotificationService>();

                        var sentCount = await notificationService.SendExpiringNotificationsAsync();
                        
                        if (sentCount > 0)
                        {
                            _logger.LogInformation($"Trimise {sentCount} notific?ri de expirare ITP");
                        }
                        else
                        {
                            _logger.LogInformation("Nu s-au trimis notific?ri (posibil toate au fost deja trimise sau nu sunt clien?i eligibili)");
                        }
                    }
                    else
                    {
                        _logger.LogInformation($"În afara orelor de lucru ({currentTime.Hour}:00). Se omite trimiterea notific?rilor.");
                        
                        // Programeaz? notific?rile pentru diminea?? dac? suntem dup? ora 21:00
                        if (currentTime.Hour >= 21)
                        {
                            using var scope = _serviceProvider.CreateScope();
                            var notificationService = scope.ServiceProvider
                                .GetRequiredService<IItpNotificationService>();
                            
                            // Aceasta va programa notific?rile pentru diminea?a urm?toare
                            await notificationService.SendExpiringNotificationsAsync();
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Eroare în Serviciul Background pentru Notific?ri ITP");
                }

                // A?teapt? urm?torul interval
                try
                {
                    await Task.Delay(_interval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Serviciul se opre?te
                    break;
                }
            }

            _logger.LogInformation("Serviciul Background pentru Notific?ri ITP s-a oprit");
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Serviciul Background pentru Notific?ri ITP se opre?te");
            await base.StopAsync(stoppingToken);
        }
    }
}