using FolderHeat.Application;
using FolderHeat.Infrastructure;

namespace FolderHeat.App;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        var repository = new SqliteFolderRepository(AppPaths.DatabasePath);
        repository.InitializeAsync().GetAwaiter().GetResult();

        var catalog = new FolderCatalogService(
            repository,
            new SystemClock(),
            new WindowsFolderLauncher());

        System.Windows.Forms.Application.Run(new TrayApplicationContext(catalog));
    }
}
