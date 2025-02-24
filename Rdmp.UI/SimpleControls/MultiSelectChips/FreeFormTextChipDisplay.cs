using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace Rdmp.UI.SimpleControls.MultiSelectChips
{
    public partial class FreeFormTextChipDisplay : UserControl
    {
        private string _value;


        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Value
        {
            get => _value;
            set
            {
                _value = value;
                flowLayoutPanel1.Controls.Clear();
                var splitValues = _value.Split(',');
                foreach (var splitValue in splitValues.Where(sv => !string.IsNullOrWhiteSpace(sv)))
                {
                    flowLayoutPanel1.Controls.Add(new Chip(splitValue, Remove));
                }
            }
        }

        private int Remove(string value)
        {
            Value = string.Join(',', Value.Split(",").Where(v => v != value));
            return 1;
        }

        public FreeFormTextChipDisplay()
        {
            InitializeComponent();
        }

        private void textBox1_KeyPressed(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                if (!_value.Split(',').Contains(textBox1.Text))
                {
                    Value = Value + $"{(Value.Length > 0 ? "," : "")}{textBox1.Text.Trim()}";
                }
                textBox1.Text = "";
            }
        }
    }
}
