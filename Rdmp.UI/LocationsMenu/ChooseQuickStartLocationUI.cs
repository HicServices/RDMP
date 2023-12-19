using Rdmp.Core.ReusableLibraryCode.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Rdmp.UI.LocationsMenu;

public partial class ChooseQuickStartLocationUI : Form
{

    public ChooseQuickStartLocationUI()
    {
        InitializeComponent();
    }

    private TextBox tbQuickStartLocation;

    private void InitializeComponent()
    {
        tbQuickStartLocation = new TextBox();
        btnConfirm = new Button();
        label1 = new Label();
        lblBadFilePath = new Label();
        lblBadFilePath.Visible = false;
        SuspendLayout();
        // 
        // tbQuickStartLocation
        // 
        tbQuickStartLocation.Location = new System.Drawing.Point(42, 65);
        tbQuickStartLocation.Name = "tbQuickStartLocation";
        tbQuickStartLocation.Size = new System.Drawing.Size(386, 23);
        tbQuickStartLocation.TabIndex = 0;
        tbQuickStartLocation.Text = "\\temp\\rdmp";
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
        // ChooseQuickStartLocationUI
        // 
        ClientSize = new System.Drawing.Size(440, 142);
        Controls.Add(lblBadFilePath);
        Controls.Add(label1);
        Controls.Add(btnConfirm);
        Controls.Add(tbQuickStartLocation);
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
        var isValid = false;
        if(string.IsNullOrEmpty(tbQuickStartLocation.Text)) isValid = false;
        else if (Directory.Exists(tbQuickStartLocation.Text)) isValid = true;
        else
        {
            try
            {
                Directory.CreateDirectory(tbQuickStartLocation.Text);
                isValid = true;
            }
            catch (Exception)
            {
                isValid = false;
            }
        }
        if (isValid)
        {
            lblBadFilePath.Visible = false;
            UserSettings.LocalFileSystemLocation = tbQuickStartLocation.Text;
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
