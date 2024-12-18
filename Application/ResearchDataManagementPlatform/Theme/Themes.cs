// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;
using Rdmp.UI.Theme;
using WeifenLuo.WinFormsUI.Docking;

namespace ResearchDataManagementPlatform.Theme;

//These classes should not be moved.  They are referenced by name in UserSettingsFileUI and UserSettings
[System.ComponentModel.DesignerCategory("")]
public class MyVS2015BlueTheme : VS2015BlueTheme, ITheme
{
    private ThemeExtender _extender;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool ApplyThemeToMenus { get; set; }

    public MyVS2015BlueTheme()
    {
        var manager = new System.Resources.ResourceManager("WeifenLuo.WinFormsUI.ThemeVS2015.Resources",
            typeof(WeifenLuo.WinFormsUI.ThemeVS2015.VS2015ThemeBase).Assembly);
        var bytes = (byte[])manager.GetObject("vs2015blue_vstheme", CultureInfo.CurrentCulture);

        _extender = new ThemeExtender(Decompress(bytes));
    }

    public new void ApplyTo(ToolStrip item)
    {
        if (ApplyThemeToMenus)
        {
            base.ApplyTo(item);
            _extender.ApplyTo(item);
        }
    }
}

public class MyVS2015DarkTheme : VS2015DarkTheme, ITheme
{
    private ThemeExtender _extender;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool ApplyThemeToMenus { get; set; }

    public MyVS2015DarkTheme()
    {
        var manager = new System.Resources.ResourceManager("WeifenLuo.WinFormsUI.ThemeVS2015.Resources",
            typeof(WeifenLuo.WinFormsUI.ThemeVS2015.VS2015ThemeBase).Assembly);
        var bytes = (byte[])manager.GetObject("vs2015dark_vstheme", CultureInfo.CurrentCulture);

        _extender = new ThemeExtender(Decompress(bytes));
    }

    public new void ApplyTo(ToolStrip item)
    {
        if (ApplyThemeToMenus)
        {
            base.ApplyTo(item);
            _extender.ApplyTo(item);
        }
    }
}

public class MyVS2015LightTheme : VS2015LightTheme, ITheme
{
    private ThemeExtender _extender;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool ApplyThemeToMenus { get; set; }

    public MyVS2015LightTheme()
    {
        var manager = new System.Resources.ResourceManager("WeifenLuo.WinFormsUI.ThemeVS2015.Resources",
            typeof(WeifenLuo.WinFormsUI.ThemeVS2015.VS2015ThemeBase).Assembly);
        var bytes = (byte[])manager.GetObject("vs2015light_vstheme", CultureInfo.CurrentCulture);

        _extender = new ThemeExtender(Decompress(bytes));
    }

    public new void ApplyTo(ToolStrip item)
    {
        if (ApplyThemeToMenus)
        {
            base.ApplyTo(item);
            _extender.ApplyTo(item);
        }
    }
}