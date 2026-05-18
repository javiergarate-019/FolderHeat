using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace FolderHeat.App;

internal sealed class AboutForm : Form
{
    private const string GitHubUrl = "https://github.com/javiergarate/FolderHeat";

    public AboutForm()
    {
        Text = "About FolderHeat";
        Icon = AppIcons.FolderHeat;
        Width = 520;
        Height = 390;
        MinimumSize = new Size(500, 370);
        StartPosition = FormStartPosition.Manual;
        MinimizeBox = false;
        MaximizeBox = false;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        Padding = new Padding(18);

        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
        };
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 96));
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        var iconBox = new PictureBox
        {
            Image = CreateLargeIconBitmap(),
            SizeMode = PictureBoxSizeMode.Zoom,
            Width = 72,
            Height = 72,
            Margin = new Padding(0, 6, 18, 0),
        };

        var contentLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 8,
        };
        contentLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        contentLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
        contentLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
        contentLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        contentLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        contentLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
        contentLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        contentLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));

        contentLayout.Controls.Add(new Label
        {
            Text = "FolderHeat",
            AutoSize = true,
            Font = new Font(Font.FontFamily, 16, FontStyle.Bold),
        }, 0, 0);

        contentLayout.Controls.Add(new Label
        {
            Text = $"Version {GetVersion()}",
            AutoSize = true,
            ForeColor = SystemColors.GrayText,
        }, 0, 1);

        contentLayout.Controls.Add(new Label
        {
            Text = "Windows tray utility that keeps your most relevant folders one shortcut away.",
            AutoSize = false,
            Dock = DockStyle.Fill,
        }, 0, 2);

        contentLayout.Controls.Add(new Label
        {
            Text = "© 2026 Javier Garate Copello",
            AutoSize = true,
        }, 0, 3);

        contentLayout.Controls.Add(new Label
        {
            Text = "MIT License",
            AutoSize = true,
        }, 0, 4);

        contentLayout.Controls.Add(new Label
        {
            Text = "GitHub:",
            AutoSize = true,
            Font = new Font(Font, FontStyle.Bold),
        }, 0, 5);

        contentLayout.Controls.Add(new LinkLabel
        {
            Text = "github.com/javiergarate/FolderHeat",
            AutoSize = true,
            LinkArea = new LinkArea(0, "github.com/javiergarate/FolderHeat".Length),
        }, 0, 6);

        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(0, 10, 0, 0),
            WrapContents = false,
        };

        var okButton = new Button
        {
            Text = "OK",
            AutoSize = true,
            DialogResult = DialogResult.OK,
        };

        var diagnosticsButton = new Button
        {
            Text = "Copy Diagnostics",
            AutoSize = true,
        };
        diagnosticsButton.Click += (_, _) => Clipboard.SetText(BuildDiagnostics());

        var githubButton = new Button
        {
            Text = "Open GitHub",
            AutoSize = true,
        };
        githubButton.Click += (_, _) => OpenGitHub();

        buttonPanel.Controls.Add(okButton);
        buttonPanel.Controls.Add(diagnosticsButton);
        buttonPanel.Controls.Add(githubButton);
        contentLayout.Controls.Add(buttonPanel, 0, 7);

        mainLayout.Controls.Add(iconBox, 0, 0);
        mainLayout.Controls.Add(contentLayout, 1, 0);

        Controls.Add(mainLayout);
        AcceptButton = okButton;
        CancelButton = okButton;
    }

    public void ShowNearCursor()
    {
        var screen = Screen.FromPoint(Cursor.Position).WorkingArea;
        Left = Math.Max(screen.Left, Math.Min(Cursor.Position.X - Width, screen.Right - Width));
        Top = Math.Max(screen.Top, Math.Min(Cursor.Position.Y - Height, screen.Bottom - Height));

        ShowDialog();
    }

    private static string GetVersion()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        return version is null ? "unknown" : $"{version.Major}.{version.Minor}.{version.Build}";
    }

    private static Bitmap CreateLargeIconBitmap()
    {
        using var icon = new Icon(AppIcons.FolderHeat, 72, 72);
        return icon.ToBitmap();
    }

    private static void OpenGitHub()
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = GitHubUrl,
            UseShellExecute = true,
        });
    }

    private static string BuildDiagnostics()
    {
        return string.Join(Environment.NewLine, new[]
        {
            $"FolderHeat {GetVersion()}",
            $"OS: {GetOsName()}",
            $"Runtime: {GetRuntimeName()}",
            $"Portable: {IsPortable()}",
            $"DB Path: {AppPaths.DatabasePath}",
        });
    }

    private static string GetOsName()
    {
        return Environment.OSVersion.Version.Build >= 22000
            ? "Windows 11"
            : RuntimeInformation.OSDescription;
    }

    private static string GetRuntimeName()
    {
        var version = Environment.Version;
        return $".NET {version.Major}";
    }

    private static bool IsPortable()
    {
        return System.Windows.Forms.Application.ExecutablePath.Contains(
            Path.Combine("artifacts", "publish"),
            StringComparison.OrdinalIgnoreCase);
    }
}
