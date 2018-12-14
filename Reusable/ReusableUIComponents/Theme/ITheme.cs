using System.Windows.Forms;

namespace ReusableUIComponents.Theme
{
    public interface ITheme
    {
        void ApplyTo(ToolStrip item);
    }
}
