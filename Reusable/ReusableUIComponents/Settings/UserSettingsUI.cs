using System;
using System.Windows.Forms;
using ReusableLibraryCode.Settings;

namespace ReusableUIComponents.Settings
{
    /// <summary>
    /// Allows you to change settings in the application that are optional e.g. whether to load the Home screen on startup or to load the state of the application when you last closed it.
    /// 
    /// <para>Settings are stored in AppData in a folder called RDMP in a file called UserSettings.txt</para>
    /// </summary>
    public partial class UserSettingsFileUI : Form
    {
        
        public UserSettingsFileUI()
        {
            InitializeComponent();
            cbShowHomeOnStartup.Checked = UserSettings.ShowHomeOnStartup;
            cbEmphasiseOnTabChanged.Checked = UserSettings.EmphasiseOnTabChanged;
            cbConfirmExit.Checked = UserSettings.ConfirmApplicationExiting;
        }

        private void cb_CheckedChanged(object sender, EventArgs e)
        {
            var cb = (CheckBox)sender;

            if (cb == cbShowHomeOnStartup)
                UserSettings.ShowHomeOnStartup = cb.Checked;

            if (cb == cbEmphasiseOnTabChanged)
                UserSettings.EmphasiseOnTabChanged = cb.Checked;

            if(cb == cbConfirmExit)
                UserSettings.ConfirmApplicationExiting = cb.Checked;

            if (cb == cbUseCaching)
                UserSettings.UseCaching = cb.Checked;
        }
    }
}
