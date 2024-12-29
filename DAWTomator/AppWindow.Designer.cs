namespace DAWTomator;

partial class AppWindow
{
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        SystemTrayIcon = new NotifyIcon(components);
        btnFilter = new Button();
        tbFilter = new TextBox();
        panel1 = new Panel();
        devicesListView = new ListView();
        theColumn = new ColumnHeader();
        panel1.SuspendLayout();
        SuspendLayout();
        // 
        // SystemTrayIcon
        // 
        SystemTrayIcon.BalloonTipIcon = ToolTipIcon.Info;
        SystemTrayIcon.Visible = true;
        SystemTrayIcon.MouseDoubleClick += SystemTrayIconDoubleClick;
        // 
        // btnFilter
        // 
        btnFilter.Dock = DockStyle.Right;
        btnFilter.Location = new Point(895, 0);
        btnFilter.Name = "btnFilter";
        btnFilter.Size = new Size(75, 40);
        btnFilter.TabIndex = 0;
        btnFilter.Text = "Filter";
        btnFilter.UseVisualStyleBackColor = true;
        btnFilter.Click += FilterClick;
        // 
        // tbFilter
        // 
        tbFilter.Location = new Point(3, 10);
        tbFilter.Name = "tbFilter";
        tbFilter.Size = new Size(696, 23);
        tbFilter.TabIndex = 2;
        tbFilter.Text = "intel(r) wireless bluetooth,intel(r) wi-fi,microsoft ac adapter,microsoft acpi-compliant control,realtek pcie gbe";
        // 
        // panel1
        // 
        panel1.Controls.Add(btnFilter);
        panel1.Controls.Add(tbFilter);
        panel1.Dock = DockStyle.Top;
        panel1.Location = new Point(0, 0);
        panel1.Name = "panel1";
        panel1.Size = new Size(970, 40);
        panel1.TabIndex = 3;
        // 
        // devicesListView
        // 
        devicesListView.CheckBoxes = true;
        devicesListView.Columns.AddRange(new ColumnHeader[] { theColumn });
        devicesListView.Dock = DockStyle.Fill;
        devicesListView.HeaderStyle = ColumnHeaderStyle.None;
        devicesListView.Location = new Point(0, 40);
        devicesListView.Name = "devicesListView";
        devicesListView.Size = new Size(970, 400);
        devicesListView.TabIndex = 4;
        devicesListView.UseCompatibleStateImageBehavior = false;
        devicesListView.View = View.Details;
        devicesListView.ItemCheck += devicesListView_ItemCheck;
        // 
        // AppWindow
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(970, 440);
        Controls.Add(devicesListView);
        Controls.Add(panel1);
        Margin = new Padding(4, 3, 4, 3);
        Name = "AppWindow";
        panel1.ResumeLayout(false);
        panel1.PerformLayout();
        ResumeLayout(false);
    }

    #endregion

    private System.Windows.Forms.NotifyIcon SystemTrayIcon;
    private Button btnFilter;
    private TextBox tbFilter;
    private Panel panel1;
    private ListView devicesListView;
    private ColumnHeader theColumn;
}

