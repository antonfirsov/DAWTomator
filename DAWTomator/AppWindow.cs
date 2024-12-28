using System.Diagnostics;
using System.Windows.Forms;
namespace DAWTomator;

public partial class AppWindow : Form
{
    public AppWindow()
    {
        InitializeComponent();
        this.CenterToScreen();

        // To provide your own custom icon image, go to:
        //   1. Project > Properties... > Resources
        //   2. Change the resource filter to icons
        //   3. Remove the Default resource and add your own
        //   4. Modify the next line to Properties.Resources.<YourResource>
        //this.Icon = Properties.Resources.Default;
        Icon icon = new Icon(Path.Combine(Environment.CurrentDirectory, "Icon.ico"));
        this.Icon = icon;
        this.SystemTrayIcon.Icon = icon;

        // Change the Text property to the name of your application
        this.SystemTrayIcon.Text = "System Tray App";
        this.SystemTrayIcon.Visible = true;

        // Modify the right-click menu of your system tray icon here
        ContextMenuStrip menu = new ContextMenuStrip();
        ToolStripItem exit = menu.Items.Add("Exit");
        exit.Click += ContextMenuExit;

        ToolStripItem test = menu.Items.Add("Test");
        test.Click += Test_Click;

        this.SystemTrayIcon.ContextMenuStrip = menu;

        this.Resize += WindowResize;
        this.FormClosing += WindowClosing;
    }

    private void Test_Click(object? sender, EventArgs e)
    {
        Debug.WriteLine("TEST!");
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
}
