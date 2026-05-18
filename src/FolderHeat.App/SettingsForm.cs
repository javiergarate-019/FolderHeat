namespace FolderHeat.App;

internal sealed class SettingsForm : Form
{
    private readonly CheckBox ctrlBox;
    private readonly CheckBox altBox;
    private readonly CheckBox shiftBox;
    private readonly CheckBox winBox;
    private readonly CheckBox startWithWindowsBox;
    private readonly ComboBox keyBox;

    public SettingsForm(HotkeySettings settings, bool startWithWindows)
    {
        Text = "FolderHeat settings";
        Icon = AppIcons.FolderHeat;
        Width = 420;
        Height = 320;
        StartPosition = FormStartPosition.CenterScreen;
        MinimizeBox = false;
        MaximizeBox = false;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        Padding = new Padding(10);

        ctrlBox = new CheckBox { Text = "Ctrl", Checked = settings.Modifiers.HasFlag(HotkeyModifiers.Control), AutoSize = true };
        altBox = new CheckBox { Text = "Alt", Checked = settings.Modifiers.HasFlag(HotkeyModifiers.Alt), AutoSize = true };
        shiftBox = new CheckBox { Text = "Shift", Checked = settings.Modifiers.HasFlag(HotkeyModifiers.Shift), AutoSize = true };
        winBox = new CheckBox { Text = "Win", Checked = settings.Modifiers.HasFlag(HotkeyModifiers.Windows), AutoSize = true };

        keyBox = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 120,
        };
        keyBox.Items.AddRange(new object[]
        {
            Keys.Space,
            Keys.F1,
            Keys.F2,
            Keys.F3,
            Keys.F4,
            Keys.F5,
            Keys.F6,
            Keys.F7,
            Keys.F8,
            Keys.F9,
            Keys.F10,
            Keys.F11,
            Keys.F12,
        });
        keyBox.SelectedItem = settings.Key;

        var modifierPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            Padding = new Padding(0, 2, 0, 0),
        };
        modifierPanel.Controls.Add(ctrlBox);
        modifierPanel.Controls.Add(altBox);
        modifierPanel.Controls.Add(shiftBox);
        modifierPanel.Controls.Add(winBox);

        var shortcutLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            Padding = new Padding(10, 8, 10, 8),
        };
        shortcutLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 72));
        shortcutLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        shortcutLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        shortcutLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        shortcutLayout.Controls.Add(new Label { Text = "Modifiers", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0);
        shortcutLayout.Controls.Add(modifierPanel, 1, 0);
        shortcutLayout.Controls.Add(new Label { Text = "Key", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 1);
        shortcutLayout.Controls.Add(keyBox, 1, 1);

        var shortcutGroup = new GroupBox
        {
            Text = "Shortcut",
            Dock = DockStyle.Top,
            Height = 104,
            Padding = new Padding(6),
        };
        shortcutGroup.Controls.Add(shortcutLayout);

        startWithWindowsBox = new CheckBox
        {
            Text = "Start FolderHeat with Windows",
            Checked = startWithWindows,
            Dock = DockStyle.Fill,
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleLeft,
        };

        var startupLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 1,
            Padding = new Padding(10, 8, 10, 8),
        };
        startupLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        startupLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        startupLayout.Controls.Add(startWithWindowsBox, 0, 0);

        var startupGroup = new GroupBox
        {
            Text = "Startup",
            Dock = DockStyle.Top,
            Height = 82,
            Padding = new Padding(6),
        };
        startupGroup.Controls.Add(startupLayout);

        var saveButton = new Button
        {
            Text = "Save",
            AutoSize = true,
            DialogResult = DialogResult.OK,
        };
        ConfigureButton(saveButton, UiIconKind.Settings);
        saveButton.Click += (_, _) =>
        {
            if (!ctrlBox.Checked && !altBox.Checked && !shiftBox.Checked && !winBox.Checked)
            {
                MessageBox.Show(
                    "Select at least one modifier.",
                    "FolderHeat",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                DialogResult = DialogResult.None;
                return;
            }

            SelectedHotkey = BuildHotkey();
            StartWithWindows = startWithWindowsBox.Checked;
            Close();
        };

        var cancelButton = new Button
        {
            Text = "Cancel",
            AutoSize = true,
            DialogResult = DialogResult.Cancel,
        };
        ConfigureButton(cancelButton, UiIconKind.Ignore);

        var bottomPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            FlowDirection = FlowDirection.RightToLeft,
            Height = 42,
            Padding = new Padding(0, 8, 0, 0),
        };
        bottomPanel.Controls.Add(saveButton);
        bottomPanel.Controls.Add(cancelButton);

        Controls.Add(bottomPanel);
        Controls.Add(startupGroup);
        Controls.Add(shortcutGroup);

        AcceptButton = saveButton;
        CancelButton = cancelButton;
        SelectedHotkey = settings;
        StartWithWindows = startWithWindows;
    }

    public HotkeySettings SelectedHotkey { get; private set; }

    public bool StartWithWindows { get; private set; }

    private HotkeySettings BuildHotkey()
    {
        var modifiers = default(HotkeyModifiers);
        if (ctrlBox.Checked)
        {
            modifiers |= HotkeyModifiers.Control;
        }

        if (altBox.Checked)
        {
            modifiers |= HotkeyModifiers.Alt;
        }

        if (shiftBox.Checked)
        {
            modifiers |= HotkeyModifiers.Shift;
        }

        if (winBox.Checked)
        {
            modifiers |= HotkeyModifiers.Windows;
        }

        return new HotkeySettings
        {
            Key = keyBox.SelectedItem is Keys key ? key : Keys.Space,
            Modifiers = modifiers,
        };
    }

    private static void ConfigureButton(Button button, UiIconKind iconKind)
    {
        button.Image = UiIconFactory.Create(iconKind);
        button.ImageAlign = ContentAlignment.MiddleLeft;
        button.TextImageRelation = TextImageRelation.ImageBeforeText;
        button.FlatStyle = FlatStyle.Standard;
        button.Padding = new Padding(4, 0, 6, 0);
        button.Height = 28;
    }
}
