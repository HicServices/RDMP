using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LoadModules.GenericUIs.DataFlowSources
{
    /// <summary>
    /// Allows you to specify an explicit C# datatype for an RDMP data flow component to use for a given named column.  For example if you are trying to load a CSV file with values 
    /// like "291","195" but they know that some codes have leading zeros "012" and wish to preserve this leading 0s so they can explicitly define the column as being a string. 
    /// </summary>
    public partial class ExplicitColumnTypeUI : UserControl
    {

        public string ColumnName {
            get { return textBox1.Text; }
        }
        public Type Type
        {
            get { return (Type) ddType.SelectedItem; }
        }

        public ExplicitColumnTypeUI(string name, Type t)
        {
            InitializeComponent();

            ddType.Items.AddRange(
                new []
                {
                    typeof(string),
                    typeof(double),
                    typeof(DateTime)
                });

            textBox1.Text = name;
            ddType.SelectedItem = t;

        }

        public event EventHandler DeletePressed;

        private void btnDelete_Click(object sender, EventArgs e)
        {
            DeletePressed(this, new EventArgs());
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
