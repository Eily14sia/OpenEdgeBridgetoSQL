using Microsoft.Extensions.Logging;
using OpenEdgeBridge.Models;
using OpenEdgeBridge.Repositories;

namespace OpenEdgeBridge.Services;

public class SyncService
{
    private readonly OpenEdgeRepository _openEdgeRepo;
    private readonly MySqlRepository _mySqlRepo;
    private readonly ILogger<SyncService> _logger;
    private const int MaxRetries = 3;

    public SyncService(
        OpenEdgeRepository openEdgeRepo,
        MySqlRepository mySqlRepo,
        ILogger<SyncService> logger)
    {
        _openEdgeRepo = openEdgeRepo;
        _mySqlRepo = mySqlRepo;
        _logger = logger;
    }

    public async Task SyncOpenEdgeToMySqlAsync()
    {
        _logger.LogInformation("Starting OpenEdge → MySQL synchronization...");

        int synced = 0;
        int failed = 0;

        try
        {
            List<Cardholder> openEdgeCards = await RetryAsync(
                () => _openEdgeRepo.GetCardholdersAsync(),
                "Read from OpenEdge");

            foreach (var card in openEdgeCards)
            {
                try
                {
                    await RetryAsync(
                        () => _mySqlRepo.UpsertCardholderAsync(card),
                        $"Upsert cardholder {card.EmployeeId}");
                    synced++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to upsert cardholder {EmployeeId}", card.EmployeeId);
                    failed++;
                }
            }

            _logger.LogInformation(
                "OpenEdge → MySQL sync complete. {Synced} records synchronized, {Failed} failed.",
                synced, failed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OpenEdge → MySQL synchronization failed.");
        }
    }

    public async Task SyncPrintCountBackToOpenEdgeAsync()
    {
        _logger.LogInformation("Starting print count sync back to OpenEdge...");

        int updated = 0;
        int failed = 0;

        try
        {
            List<Cardholder> mySqlCards = await RetryAsync(
                () => _mySqlRepo.GetPendingPrintsAsync(),
                "Read from MySQL");

            List<Cardholder> openEdgeCards = await RetryAsync(
                () => _openEdgeRepo.GetCardholdersAsync(),
                "Read from OpenEdge for print count comparison");

            // Build lookup by EmployeeId for fast comparison
            var openEdgeLookup = openEdgeCards
                .Where(c => !string.IsNullOrEmpty(c.EmployeeId))
                .ToDictionary(c => c.EmployeeId, c => c.PrintCount);

            foreach (var mysqlCard in mySqlCards)
            {
                if (string.IsNullOrEmpty(mysqlCard.EmployeeId))
                    continue;

                if (openEdgeLookup.TryGetValue(mysqlCard.EmployeeId, out int oePrintCount))
                {
                    // If MySQL has a higher print count, push it back to OpenEdge
                    if (mysqlCard.PrintCount > oePrintCount)
                    {
                        try
                        {
                            int diff = mysqlCard.PrintCount - oePrintCount;
                            for (int i = 0; i < diff; i++)
                            {
                                await RetryAsync(
                                    () => _openEdgeRepo.UpdatePrintCountAsync(mysqlCard.Id),
                                    $"Update print count for {mysqlCard.EmployeeId}");
                            }
                            updated++;
                            _logger.LogInformation(
                                "Synced print count for {EmployeeId}: OpenEdge {OE} → MySQL {MySQL}",
                                mysqlCard.EmployeeId, oePrintCount, mysqlCard.PrintCount);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to update print count for {EmployeeId}", mysqlCard.EmployeeId);
                            failed++;
                        }
                    }
                }
            }

            _logger.LogInformation(
                "Print count sync complete. {Updated} records updated, {Failed} failed.",
                updated, failed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Print count back-sync failed.");
        }
    }

    private static async Task<T> RetryAsync<T>(
        Func<Task<T>> operation,
        string description,
        int maxRetries = MaxRetries)
    {
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                return await operation();
            }
            catch (Exception) when (attempt < maxRetries)
            {
                int delayMs = attempt * 1000;
                await Task.Delay(delayMs);
            }
        }

        // Final attempt — let exception propagate
        return await operation();
    }

    private static async Task RetryAsync(
        Func<Task> operation,
        string description,
        int maxRetries = MaxRetries)
    {
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                await operation();
                return;
            }
            catch (Exception) when (attempt < maxRetries)
            {
                int delayMs = attempt * 1000;
                await Task.Delay(delayMs);
            }
        }

        // Final attempt — let exception propagate
        await operation();
    }
}
