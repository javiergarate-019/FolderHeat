namespace FolderHeat.App;

internal sealed class SettingsForm : Form
{
    private readonly CheckBox ctrlBox;
    private readonly CheckBox altBox;
    private readonly CheckBox shiftBox;
    private readonly CheckBox winBox;
    private readonly ComboBox keyBox;

    public SettingsForm(HotkeySettings settings)
    {
        Text = "FolderHeat settings";
        Width = 360;
        Height = 220;
        StartPosition = FormStartPosition.CenterScreen;
        MinimizeBox = false;
        MaximizeBox = false;
        FormBorderStyle = FormBorderStyle.FixedDialog;

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
            Dock = DockStyle.Top,
            Height = 34,
            Padding = new Padding(8, 4, 8, 4),
        };
        modifierPanel.Controls.Add(ctrlBox);
        modifierPanel.Controls.Add(altBox);
        modifierPanel.Controls.Add(shiftBox);
        modifierPanel.Controls.Add(winBox);

        var keyPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 42,
            Padding = new Padding(8),
        };
        keyPanel.Controls.Add(new Label { Text = "Key", AutoSize = true, Padding = new Padding(0, 6, 8, 0) });
        keyPanel.Controls.Add(keyBox);

        var saveButton = new Button
        {
            Text = "Save",
            AutoSize = true,
            DialogResult = DialogResult.OK,
        };
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
            Close();
        };

        var cancelButton = new Button
        {
            Text = "Cancel",
            AutoSize = true,
            DialogResult = DialogResult.Cancel,
        };

        var bottomPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            FlowDirection = FlowDirection.RightToLeft,
            Height = 42,
            Padding = new Padding(8),
        };
        bottomPanel.Controls.Add(saveButton);
        bottomPanel.Controls.Add(cancelButton);

        Controls.Add(bottomPanel);
        Controls.Add(keyPanel);
        Controls.Add(modifierPanel);

        AcceptButton = saveButton;
        CancelButton = cancelButton;
        SelectedHotkey = settings;
    }

    public HotkeySettings SelectedHotkey { get; private set; }

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
}
