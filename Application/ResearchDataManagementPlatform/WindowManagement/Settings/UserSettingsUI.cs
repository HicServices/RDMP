using System;
using System.Windows.Forms;

namespace ResearchDataManagementPlatform.WindowManagement.Settings
{
    /// <summary>
    /// Allows you to change settings in the application that are optional e.g. whether to load the Home screen on startup or to load the state of the application when you last closed it.
    /// 
    /// Settings are stored in AppData in a folder called RDMP in a file called UserSettings.txt
    /// </summary>
    public partial class UserSettingsFileUI : Form
    {
        
        public UserSettingsFileUI()
        {
            InitializeComponent();
            cbShowHomeOnStartup.Checked = UserSettings.ShowHomeOnStartup;
            cbEmphasiseOnTabChanged.Checked = UserSettings.EmphasiseOnTabChanged;
        }

        private void cbShowHomeOnStartup_CheckedChanged(object sender, EventArgs e)
        {
            UserSettings.ShowHomeOnStartup = cbShowHomeOnStartup.Checked;
        }

        private void cbEmphasiseOnTabChanged_CheckedChanged(object sender, EventArgs e)
        {
            UserSettings.EmphasiseOnTabChanged = cbEmphasiseOnTabChanged.Checked;
        }
    }
}
