using System.Windows.Forms;

namespace CatalogueManager.PluginManagement.CodeGeneration
{
    /// <summary>
    /// TECHNICAL: Allows you as a C# programmer to generate the class code from a table that will inherit from DatabaseEntity.  This lets you create new objects and have the 
    /// database record automatically created.  It gives you support for revert, save, check still exists, delete, property change events etc.
    /// </summary>
    public partial class PluginCodeGeneration : Form
    {
        public PluginCodeGeneration()
        {
            InitializeComponent();
        }
    }
}
