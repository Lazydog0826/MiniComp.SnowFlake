namespace MiniComp.SnowFlake;

public static class SnowFlakeConfiguration
{
    /// <summary>
    /// 默认值6，限制每毫秒生成的ID个数。若生成速度超过5万个/秒，建议加大 SeqBitLength 到 10
    /// </summary>
    internal static byte SeqBitLength;

    /// <summary>
    /// 默认值6，限定 WorkerId 最大值为2^6-1，即默认最多支持64个节点
    /// </summary>
    internal static byte WorkerIdBitLength;

    /// <summary>
    /// 基础时间
    /// </summary>
    internal static DateTime BaseTime;

    /// <summary>
    ///工作ID缓存KEY
    /// </summary>
    internal static string WorkIdKey = string.Empty;

    /// <summary>
    /// 占用更新间隔
    /// </summary>
    internal static long OccupyUpdateInterval;

    /// <summary>
    /// 占用过期时间
    /// </summary>
    internal static long OccupyExpireTime;

    /// <summary>
    /// 是否已设置BitLength
    /// </summary>
    private static bool _isSet;

    /// <summary>
    /// WorkId
    /// </summary>
    internal static ushort WorkId = 0;

    public static void SetOption(
        byte seqBitLength = 6,
        byte workerIdBitLength = 6,
        DateTime baseTime = new(),
        long occupyUpdateInterval = 60000,
        long occupyExpireTime = 60000 + 10000
    )
    {
        if (_isSet)
        {
            throw new Exception("请勿重复设置");
        }
        _isSet = true;
        SeqBitLength = seqBitLength;
        WorkerIdBitLength = workerIdBitLength;
        BaseTime = baseTime;
        OccupyUpdateInterval = occupyUpdateInterval;
        OccupyExpireTime = occupyExpireTime;
    }
}
