using FolderHeat.Application;

namespace FolderHeat.Infrastructure;

public sealed class SystemClock : IClock
{
    public DateTimeOffset Now => DateTimeOffset.Now;
}
