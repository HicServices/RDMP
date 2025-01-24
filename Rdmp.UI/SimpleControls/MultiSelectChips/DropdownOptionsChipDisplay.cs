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


        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Value
        {
            get => _value;
            set
            {
                _value = value;
                flowLayoutPanel1.Controls.Clear();
                var splitValues = _value.Split(',');
                comboBox1.Items.Clear();
                foreach (var option in _options.Where(o => !splitValues.Contains(o)).ToList())
                {
                    comboBox1.Items.Add(option);
                }
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
                comboBox1.Items.Clear();
                foreach (var option in _options)
                {
                    comboBox1.Items.Add(option);
                }
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


        public void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var value = comboBox1.SelectedItem as string;
            Value = Value + $"{(Value.Length>0?",":"")}{value}";
        }
    }
}
