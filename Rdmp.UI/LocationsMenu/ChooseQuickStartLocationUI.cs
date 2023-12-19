using Rdmp.Core.ReusableLibraryCode.Settings;
using System;
using System.Collections.Generic;
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
        SuspendLayout();
        // 
        // tbQuickStartLocation
        // 
        tbQuickStartLocation.Location = new System.Drawing.Point(42, 65);
        tbQuickStartLocation.Name = "tbQuickStartLocation";
        tbQuickStartLocation.Size = new System.Drawing.Size(386, 23);
        tbQuickStartLocation.TabIndex = 0;
        tbQuickStartLocation.Text = UserSettings.QuickStartLocation;
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
        // ChooseQuickStartLocationUI
        // 
        ClientSize = new System.Drawing.Size(440, 142);
        Controls.Add(label1);
        Controls.Add(btnConfirm);
        Controls.Add(tbQuickStartLocation);
        Name = "ChooseQuickStartLocationUI";
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
        //todo validate
        UserSettings.QuickStartLocation = tbQuickStartLocation.Text;
        UserSettings.UseQuickStartSettings = true;
        ApplicationRestarter.Restart();
    }
}
