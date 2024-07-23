using Rdmp.Core.ReusableLibraryCode.Settings;
using System;
using System.IO;
using System.Windows.Forms;

namespace Rdmp.UI.LocationsMenu;

public partial class ChooseLocalFileSystemLocationUI : Form
{

    public ChooseLocalFileSystemLocationUI()
    {
        InitializeComponent();
    }

    private TextBox tbLocalFileSystemLocation;

    private void InitializeComponent()
    {
        tbLocalFileSystemLocation = new TextBox();
        btnConfirm = new Button();
        label1 = new Label();
        lblBadFilePath = new Label();
        lblBadFilePath.Visible = false;
        this.Text = "Choose Local File System Location";
        SuspendLayout();
        // 
        // tbLocalFileSystemLocation
        // 
        tbLocalFileSystemLocation.Location = new System.Drawing.Point(42, 65);
        tbLocalFileSystemLocation.Name = "tbLocalFileSystemLocation";
        tbLocalFileSystemLocation.Size = new System.Drawing.Size(386, 23);
        tbLocalFileSystemLocation.TabIndex = 0;
        tbLocalFileSystemLocation.Text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "rdmp");
        // 
        // btnConfirm
        // 
        btnConfirm.Location = new System.Drawing.Point(353, 107);
        btnConfirm.Name = "btnConfirm";
        btnConfirm.Size = new System.Drawing.Size(75, 23);
        btnConfirm.TabIndex = 1;
        btnConfirm.Text = "Confirm";
        btnConfirm.UseVisualStyleBackColor = true;
        btnConfirm.Click += btnConfirm_Click;
        // 
        // label1
        // 
        label1.AutoSize = true;
        label1.Location = new System.Drawing.Point(43, 45);
        label1.Name = "label1";
        label1.Size = new System.Drawing.Size(161, 15);
        label1.TabIndex = 3;
        label1.Text = "Location To Store RDMP Data";
        label1.Click += label1_Click;
        // 
        // lblBadFilePath
        // 
        lblBadFilePath.AutoSize = true;
        lblBadFilePath.ForeColor = System.Drawing.Color.Red;
        lblBadFilePath.Location = new System.Drawing.Point(210, 45);
        lblBadFilePath.Name = "lblBadFilePath";
        lblBadFilePath.Size = new System.Drawing.Size(90, 15);
        lblBadFilePath.TabIndex = 4;
        lblBadFilePath.Text = "Invalid File Path";
        // 
        // ChooseLocalFileStorageLocationUI
        // 
        ClientSize = new System.Drawing.Size(440, 142);
        Controls.Add(lblBadFilePath);
        Controls.Add(label1);
        Controls.Add(btnConfirm);
        Controls.Add(tbLocalFileSystemLocation);
        Name = "Choose Local File Storage Location";
        ResumeLayout(false);
        PerformLayout();
    }

    private Button btnConfirm;
    private Label label1;

    private void label1_Click(object sender, EventArgs e)
    {

    }

    private void btnConfirm_Click(object sender, EventArgs e)
    {
        bool isValid;
        if (string.IsNullOrEmpty(tbLocalFileSystemLocation.Text)) isValid = false;
        else if (Directory.Exists(tbLocalFileSystemLocation.Text)) isValid = true;
        else
        {
            Directory.CreateDirectory(tbLocalFileSystemLocation.Text);
            isValid = true;
        }

        if (isValid)
        {
            lblBadFilePath.Visible = false;
            UserSettings.LocalFileSystemLocation = tbLocalFileSystemLocation.Text;
            UserSettings.UseLocalFileSystem = true;
            ApplicationRestarter.Restart();
        }
        else
        {
            lblBadFilePath.Visible = true;
        }

    }

    private Label lblBadFilePath;
}
