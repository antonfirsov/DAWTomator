using DAWTomator.Properties;

namespace DAWTomator;

public partial class AppWindow : Form
{
    private record class DeviceItem(DeviceInfo Device, ListViewItem ListViewItem, ToolStripMenuItem ToolStripItem)
    {
        public bool TrySetEnabled(bool enabled)
        {
            try
            {
                HardwareManager.SetEnabled(Device, enabled);
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show($"Access denied on {Device}!");
                return false;
            }
        }
    }

    public AppWindow()
    {
        InitializeComponent();

        string defaultDevicesFile = Path.Combine(Environment.CurrentDirectory, "DefaultDevices.csv");
        if (File.Exists(defaultDevicesFile))
        {
            tbFilter.Text = File.ReadAllText(defaultDevicesFile);
        }
        else
        {
            tbFilter.Text = "intel(r) wi-fi";
        }

        this.CenterToScreen();

        this.Icon = Resources.Icon;
        this.SystemTrayIcon.Icon = Resources.Icon;

        this.SystemTrayIcon.Text = "🍅 DAWTomator 🍅";
        this.SystemTrayIcon.Visible = true;

        this.Resize += WindowResize;
        this.FormClosing += WindowClosing;
        InitDeviceLists();

        this.SystemTrayIcon.ContextMenuStrip = systemTrayMenu;

        this.WindowState = FormWindowState.Minimized;
        this.Hide();
    }

    private DeviceInfo[] GetDevices()
    {
        string[] keywords = tbFilter.Text.Split(',').Select(s => s.Trim()).ToArray();
        return HardwareManager.GetDevices()
            .FilterKeywords(keywords)
            .OrderBy(d => d.ToString())
            .ToArray();
    }

    private void InitDeviceLists()
    {
        devicesListView.ItemCheck -= devicesListView_ItemCheck;
        devicesListView.Items.Clear();
        systemTrayMenu.Items.Clear();

        foreach (DeviceInfo device in GetDevices())
        {
            string text = device.ToString();
            ListViewItem listViewItem = new ListViewItem(text)
            {
                Checked = device.Enabled == true
            };
            ToolStripMenuItem toolStripItem = (ToolStripMenuItem)systemTrayMenu.Items.Add(text);
            toolStripItem.Checked = device.Enabled == true;
            
            DeviceItem deviceItem = new(device, listViewItem, toolStripItem);
            listViewItem.Tag = deviceItem;
            toolStripItem.Tag = deviceItem;
            toolStripItem.DisplayStyle = ToolStripItemDisplayStyle.Text;

            toolStripItem.Click += this.ToolStripItemClicked;

            devicesListView.Items.Add(listViewItem);
            
        }
        devicesListView.AutoResizeColumn(0, ColumnHeaderAutoResizeStyle.ColumnContent);
        devicesListView.ItemCheck += devicesListView_ItemCheck;

        systemTrayMenu.Items.Add("-");
        ToolStripItem exit = systemTrayMenu.Items.Add("Exit");
        exit.Click += ContextMenuExit;
    }

    private void FilterClick(object? sender, EventArgs e)
    {
        InitDeviceLists();
    }

    private void SystemTrayIconDoubleClick(object? sender, MouseEventArgs e)
    {
        this.WindowState = FormWindowState.Minimized;
        this.Show();
        this.WindowState = FormWindowState.Normal;
    }

    private void ContextMenuExit(object? sender, EventArgs e)
    {
        this.SystemTrayIcon.Visible = false;
        Application.Exit();
        Environment.Exit(0);
    }

    private void WindowResize(object? sender, EventArgs e)
    {
        if (this.WindowState == FormWindowState.Minimized)
        {
            this.Hide();
        }
    }

    private void WindowClosing(object? sender, FormClosingEventArgs e)
    {
        e.Cancel = true;
        this.Hide();
    }

    private void ToolStripItemClicked(object? sender, EventArgs e)
    {
        ToolStripMenuItem btn = (ToolStripMenuItem)sender!;
        DeviceItem deviceItem = (DeviceItem)btn.Tag!;
        bool enabled = !btn.Checked;

        if (deviceItem.TrySetEnabled(enabled))
        {
            btn.Checked = enabled;
            devicesListView.ItemCheck -= devicesListView_ItemCheck;
            deviceItem.ListViewItem.Checked = enabled;
            devicesListView.ItemCheck += devicesListView_ItemCheck;
        }
    }

    private void devicesListView_ItemCheck(object? sender, ItemCheckEventArgs e)
    {
        DeviceItem item = (DeviceItem)devicesListView.Items[e.Index].Tag!;
        if (e.NewValue != CheckState.Indeterminate && e.CurrentValue != e.NewValue)
        {
            bool enabled = e.NewValue == CheckState.Checked;
            if (item.TrySetEnabled(enabled))
            {
                item.ToolStripItem.Checked = enabled;
            }
        }
    }
}
