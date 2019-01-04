using System.Windows.Forms;
using ReusableUIComponents.Theme;
using WeifenLuo.WinFormsUI.Docking;

namespace ResearchDataManagementPlatform
{
    //These classes should not be moved.  They are referenced by name in UserSettingsFileUI and UserSettings

    public class MyVS2015BlueTheme : VS2015BlueTheme, ITheme
    {
        public bool ApplyThemeToMenus { get; set; }

        public new void ApplyTo(ToolStrip item)
        {
            if(ApplyThemeToMenus)
                base.ApplyTo(item);
        }
    }
    public class MyVS2015DarkTheme : VS2015DarkTheme, ITheme
    {
        public bool ApplyThemeToMenus { get; set; }

        public new void ApplyTo(ToolStrip item)
        {
            if (ApplyThemeToMenus)
                base.ApplyTo(item);
        }
    }
    public class MyVS2015LightTheme : VS2015LightTheme, ITheme
    {
        public bool ApplyThemeToMenus { get; set; }

        public new void ApplyTo(ToolStrip item)
        {
            if (ApplyThemeToMenus)
                base.ApplyTo(item);
        }
    }
}