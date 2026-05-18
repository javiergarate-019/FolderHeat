using FolderHeat.Application;
using FolderHeat.Infrastructure;

namespace FolderHeat.App;

static class Program
{
    private const string SingleInstanceMutexName = @"Local\FolderHeat.SingleInstance";

    [STAThread]
    static void Main()
    {
        using var singleInstanceMutex = new Mutex(true, SingleInstanceMutexName, out var ownsMutex);
        if (!ownsMutex)
        {
            return;
        }

        ApplicationConfiguration.Initialize();

        var repository = new SqliteFolderRepository(AppPaths.DatabasePath);
        repository.InitializeAsync().GetAwaiter().GetResult();

        var catalog = new FolderCatalogService(
            repository,
            new SystemClock(),
            new WindowsFolderLauncher(),
            new CompositeActiveFolderSource(
                new ExplorerActiveFolderSource(),
                new VsCodeActiveFolderSource(),
                new NotepadActiveFolderSource(),
                new NotepadPlusPlusActiveFolderSource(),
                new WindowsRecentFolderSource()));

        System.Windows.Forms.Application.Run(new TrayApplicationContext(
            catalog,
            new AppSettingsStore(AppPaths.SettingsPath)));
    }
}
