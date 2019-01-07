using System.Globalization;
using System.Windows.Forms;
using ReusableUIComponents.Theme;
using WeifenLuo.WinFormsUI.Docking;

namespace ResearchDataManagementPlatform.Theme
{
    //These classes should not be moved.  They are referenced by name in UserSettingsFileUI and UserSettings
    [System.ComponentModel.DesignerCategory("")]
    public class MyVS2015BlueTheme : VS2015BlueTheme, ITheme
    {
        private ThemeExtender _extender;
        public bool ApplyThemeToMenus { get; set; }
        
        public MyVS2015BlueTheme()
        {
            var manager = new System.Resources.ResourceManager("WeifenLuo.WinFormsUI.ThemeVS2015.Resources", typeof(WeifenLuo.WinFormsUI.ThemeVS2015.VS2015ThemeBase).Assembly);
            byte[] bytes = (byte[]) manager.GetObject("vs2015blue_vstheme",CultureInfo.CurrentCulture);

            _extender = new ThemeExtender(Decompress(bytes));
        }

        public new void ApplyTo(ToolStrip item)
        {
            if(ApplyThemeToMenus)
            {
                base.ApplyTo(item);
                _extender.ApplyTo(item);
            }
        }
    }

    public class MyVS2015DarkTheme : VS2015DarkTheme, ITheme
    {
        private ThemeExtender _extender;
        public bool ApplyThemeToMenus { get; set; }

        public MyVS2015DarkTheme()
        {
            var manager = new System.Resources.ResourceManager("WeifenLuo.WinFormsUI.ThemeVS2015.Resources", typeof(WeifenLuo.WinFormsUI.ThemeVS2015.VS2015ThemeBase).Assembly);
            byte[] bytes = (byte[])manager.GetObject("vs2015dark_vstheme", CultureInfo.CurrentCulture);

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
        public bool ApplyThemeToMenus { get; set; }

        public MyVS2015LightTheme()
        {
            var manager = new System.Resources.ResourceManager("WeifenLuo.WinFormsUI.ThemeVS2015.Resources", typeof(WeifenLuo.WinFormsUI.ThemeVS2015.VS2015ThemeBase).Assembly);
            byte[] bytes = (byte[])manager.GetObject("vs2015light_vstheme", CultureInfo.CurrentCulture);

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
}