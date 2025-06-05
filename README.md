# MiniComp.SnowFlake
依赖：[https://github.com/yitter/IdGenerator](https://github.com/yitter/IdGenerator)  和  [https://github.com/Lazydog0826/MiniComp.Cache](https://github.com/Lazydog0826/MiniComp.Cache)

```csharp
SnowFlakeConfiguration.SetOption(6, 6, default(DateTime), 60000, 70000);
builder.Services.AddHostedService<SnowFlakeHostService>();
var newId = YitIdHelper.NextId();
```
