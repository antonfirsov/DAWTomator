using System.Diagnostics;
using Windows.Win32;
namespace DAWTomator;

public partial class AppWindow : Form
{
    public AppWindow()
    {
        InitializeComponent();
        this.CenterToScreen();

        Icon icon = new Icon(Path.Combine(Environment.CurrentDirectory, "Icon.ico"));
        this.Icon = icon;
        this.SystemTrayIcon.Icon = icon;

        this.SystemTrayIcon.Text = "🍅 DAWTomator 🍅";
        this.SystemTrayIcon.Visible = true;

        ContextMenuStrip menu = new ContextMenuStrip();
        ToolStripItem exit = menu.Items.Add("Exit");
        exit.Click += ContextMenuExit;

        //ToolStripItem test = menu.Items.Add("Test");
        //test.Click += (_, __) => Debug.WriteLine("TEST$");

        this.SystemTrayIcon.ContextMenuStrip = menu;

        this.Resize += WindowResize;
        this.FormClosing += WindowClosing;
        InitDevicesListboxListbox();
    }

    private void InitDevicesListboxListbox()
    {
        string[] keywords = tbFilter.Text.Split(',').Select(s => s.Trim()).ToArray();
        DeviceInfo[] devices = HardwareManager.GetDevices()
            .FilterKeywords(keywords)
            .OrderBy(d => d.ToString())
            .ToArray();

        devicesListView.ItemCheck -= devicesListView_ItemCheck;
        devicesListView.Items.Clear();
        foreach (DeviceInfo device in devices)
        {
            ListViewItem item = new ListViewItem(device.ToString())
            {
                Checked = device.Enabled == true,
                Tag = device
            };
            devicesListView.Items.Add(item);
        }
        devicesListView.AutoResizeColumn(0, ColumnHeaderAutoResizeStyle.ColumnContent);
        devicesListView.ItemCheck += devicesListView_ItemCheck;
    }

    private void FilterClick(object? sender, EventArgs e)
    {
        InitDevicesListboxListbox();
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

    private void devicesListView_ItemCheck(object? sender, ItemCheckEventArgs e)
    {
        DeviceInfo info = (DeviceInfo)devicesListView.Items[e.Index].Tag!;
        if (e.NewValue != CheckState.Indeterminate && e.CurrentValue != e.NewValue)
        {
            try
            {
                HardwareManager.SetEnabled(info, e.NewValue == CheckState.Checked);
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show($"Access denied on {info}!");
                e.NewValue = CheckState.Indeterminate;
            }
        }
    }
}
