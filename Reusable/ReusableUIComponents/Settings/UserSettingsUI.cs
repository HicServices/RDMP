// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Windows.Forms;
using ReusableLibraryCode.Settings;
using ScintillaNET;

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
            cbFindShouldPin.Checked = UserSettings.FindShouldPin;
            cbThemeMenus.Checked = UserSettings.ApplyThemeToMenus;

            ddTheme.DataSource = new []
            {
                "ResearchDataManagementPlatform.Theme.MyVS2015BlueTheme",
                "ResearchDataManagementPlatform.Theme.MyVS2015DarkTheme",
                "ResearchDataManagementPlatform.Theme.MyVS2015LightTheme"
            };

            ddTheme.SelectedItem = UserSettings.Theme;

            ddWordWrap.DataSource = Enum.GetValues(typeof(WrapMode));
            ddWordWrap.SelectedItem = (WrapMode)UserSettings.WrapMode;

            tbHeatmapColours.Text = UserSettings.HeatMapColours;

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
            
            if (cb == cbThemeMenus)
                UserSettings.ApplyThemeToMenus = cb.Checked;

            if(cb == cbFindShouldPin)
                UserSettings.FindShouldPin = cbFindShouldPin.Checked;
        }

        private void ddTheme_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(!_bLoaded)
                return;
            
            var t = ddTheme.SelectedItem as string;
            
            if(t != null)
                UserSettings.Theme = t;
        }

        private void ddWordWrap_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_bLoaded)
                return;

            var wrap = (WrapMode)ddWordWrap.SelectedItem;
            UserSettings.WrapMode = (int)wrap;
        }

        private void TbHeatmapColours_TextChanged(object sender, EventArgs e)
        {
            UserSettings.HeatMapColours = tbHeatmapColours.Text;
        }
    }
}
