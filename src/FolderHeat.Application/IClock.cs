namespace FolderHeat.Application;

public interface IClock
{
    DateTimeOffset Now { get; }
}
