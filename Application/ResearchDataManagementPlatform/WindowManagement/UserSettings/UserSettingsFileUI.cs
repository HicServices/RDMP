using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ResearchDataManagementPlatform.WindowManagement.UserSettings
{
    /// <summary>
    /// Allows you to change settings in the application that are optional e.g. whether to load the Home screen on startup or to load the state of the application when you last closed it.
    /// 
    /// Settings are stored in AppData in a folder called RDMP in a file called UserSettings.txt
    /// </summary>
    public partial class UserSettingsFileUI : Form
    {
        private readonly UserSettingsFile _instance;
        private readonly bool _loading = true;

        public UserSettingsFileUI()
        {
            InitializeComponent();
            _instance = UserSettingsFile.GetInstance();
            cbShowHomeOnStartup.Checked = _instance.ShowHomeScreenInsteadOfPersistence;
            cbEmphasiseOnTabChanged.Checked = _instance.EmphasiseOnTabChanged;

            _loading = false;
        }

        private void cbShowHomeOnStartup_CheckedChanged(object sender, EventArgs e)
        {
            if(_loading)
                return;
            
            _instance.ShowHomeScreenInsteadOfPersistence = cbShowHomeOnStartup.Checked;

        }

        private void cbEmphasiseOnTabChanged_CheckedChanged(object sender, EventArgs e)
        {
            if (_loading)
                return;

            _instance.EmphasiseOnTabChanged = cbEmphasiseOnTabChanged.Checked;
        }
    }
}
