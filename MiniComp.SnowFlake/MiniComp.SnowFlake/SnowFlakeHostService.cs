using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MiniComp.Cache;
using Yitter.IdGenerator;

namespace MiniComp.SnowFlake;

public class SnowFlakeHostService(ILogger<SnowFlakeHostService> logger, ICacheService cacheService)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("开始分配WorkId");
        var workMax = (long)Math.Pow(2, SnowFlakeConfiguration.WorkerIdBitLength) - 1;
        const string incrementKey = "Snowflake:WorkId:IncrementKey";
        const string lockKey = "Snowflake:WorkId:LockKey";
        const string occupyKey = "Snowflake:WorkId:";
        await cacheService.LockAsync(
            lockKey,
            async () =>
            {
                var isReset = false;
                while (true)
                {
                    var value = await cacheService.GetCacheAsync<ushort?>(incrementKey);
                    if (!value.HasValue)
                    {
                        value = 0;
                    }
                    else
                    {
                        value++;
                    }
                    if (value >= workMax)
                    {
                        if (isReset)
                        {
                            throw new Exception("WorkId已被占满");
                        }
                        value = 0;
                        isReset = true;
                    }
                    await cacheService.AddCacheAsync(incrementKey, value.Value);
                    SnowFlakeConfiguration.WorkIdKey = occupyKey + value.Value;
                    if (await cacheService.IsExistAsync(SnowFlakeConfiguration.WorkIdKey))
                        continue;
                    await cacheService.AddCacheAsync(
                        SnowFlakeConfiguration.WorkIdKey,
                        0,
                        TimeSpan.FromMilliseconds(SnowFlakeConfiguration.OccupyExpireTime)
                    );
                    SnowFlakeConfiguration.WorkId = value.Value;
                    break;
                }
            }
        );
        YitIdHelper.SetIdGenerator(
            new IdGeneratorOptions()
            {
                SeqBitLength = SnowFlakeConfiguration.SeqBitLength,
                WorkerIdBitLength = SnowFlakeConfiguration.WorkerIdBitLength,
                BaseTime = SnowFlakeConfiguration.BaseTime,
                WorkerId = SnowFlakeConfiguration.WorkId,
            }
        );
        logger.LogInformation("WorkId = {WorkId}", SnowFlakeConfiguration.WorkId);
        using var periodicTimer = new PeriodicTimer(
            TimeSpan.FromMilliseconds(SnowFlakeConfiguration.OccupyUpdateInterval)
        );
        while (await periodicTimer.WaitForNextTickAsync(stoppingToken))
        {
            await cacheService.KeyExpireAsync(
                SnowFlakeConfiguration.WorkIdKey,
                TimeSpan.FromMilliseconds(SnowFlakeConfiguration.OccupyExpireTime)
            );
        }
    }
}
