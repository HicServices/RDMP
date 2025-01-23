using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Rdmp.UI.SimpleControls.MultiSelectChips
{
    public partial class DropdownOptionsChipDisplay : UserControl
    {
        private string _value;
        private string[] _options;

        private String[] defaultValueArray = [""];

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Value
        {
            get => _value;
            set
            {
                _value = value;
                flowLayoutPanel1.Controls.Clear();
                var splitValues = _value.Split(',');
                comboBox1.Text = "";
                comboBox1.DataSource = defaultValueArray.Concat(_options.Where(o => !splitValues.Contains(o)).ToList());
                foreach (var splitValue in splitValues)
                {
                    if (!string.IsNullOrWhiteSpace(splitValue))
                        flowLayoutPanel1.Controls.Add(new Chip(splitValue, Remove));
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public String[] Options
        {
            get => _options;
            set
            {
                _options = value;
                comboBox1.Text = "";
                comboBox1.DataSource = defaultValueArray.Concat(_options);
            }
        }

        private int Remove(string value)
        {
            Value = string.Join(',', Value.Split(",").Where(v => v != value));
            return 1;
        }

        public DropdownOptionsChipDisplay()
        {
            InitializeComponent();
        }


        public void comboBox1_TextChanged(object sender, EventArgs e)
        {
            if (comboBox1.Text != "")
            {
                var value = comboBox1.SelectedItem as string;
                Value = Value + $",{value}";
                comboBox1.Text = "";
            }

        }
    }
}
