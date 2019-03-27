using System.Reflection;
using System.Windows.Forms;
using CatalogueManager.TestsAndSetup.ServicePropogation;

namespace CatalogueLibraryTests.UserInterfaceTests
{
    public class UITests
    {
        protected T GetPrivateField<T>(Control c, string field)
        {
            return (T)c.GetType().GetField(field, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(c);
        }

        protected T GetSingleDatabaseObjectControlForm<T>() where T : Control, IRDMPSingleDatabaseObjectControl, new()
        {
            Form f = new Form();
            T ui = new T();
            f.Controls.Add(ui);

            return ui;
        }
    }
}