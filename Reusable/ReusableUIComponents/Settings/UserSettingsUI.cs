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
        private bool _bLoaded;

        public UserSettingsFileUI()
        {
            InitializeComponent();
            cbShowHomeOnStartup.Checked = UserSettings.ShowHomeOnStartup;
            cbEmphasiseOnTabChanged.Checked = UserSettings.EmphasiseOnTabChanged;
            cbConfirmExit.Checked = UserSettings.ConfirmApplicationExiting;
            cbUseCaching.Checked = UserSettings.UseCaching;
            cbThemeMenus.Checked = UserSettings.ApplyThemeToMenus;

            ddTheme.DataSource = new []
            {
                "ResearchDataManagementPlatform.Theme.MyVS2015BlueTheme",
                "ResearchDataManagementPlatform.Theme.MyVS2015DarkTheme",
                "ResearchDataManagementPlatform.Theme.MyVS2015LightTheme"
            };

            ddTheme.SelectedItem = UserSettings.Theme;

            _bLoaded = true;
        }

        private void cb_CheckedChanged(object sender, EventArgs e)
        {
            if (!_bLoaded)
                return;
            
            var cb = (CheckBox)sender;

            if (cb == cbShowHomeOnStartup)
                UserSettings.ShowHomeOnStartup = cb.Checked;

            if (cb == cbEmphasiseOnTabChanged)
                UserSettings.EmphasiseOnTabChanged = cb.Checked;

            if(cb == cbConfirmExit)
                UserSettings.ConfirmApplicationExiting = cb.Checked;

            if (cb == cbUseCaching)
                UserSettings.UseCaching = cb.Checked;

            if (cb == cbThemeMenus)
                UserSettings.ApplyThemeToMenus = cb.Checked;
        }

        private void ddTheme_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(!_bLoaded)
                return;
            
            var t = ddTheme.SelectedItem as string;
            
            if(t != null)
                UserSettings.Theme = t;
        }

    }
}
