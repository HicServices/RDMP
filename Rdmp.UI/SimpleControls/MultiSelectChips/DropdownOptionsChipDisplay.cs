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

        private string AddSpacesToSentence(string text, bool preserveAcronyms)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;
            StringBuilder newText = new StringBuilder(text.Length * 2);
            newText.Append(text[0]);
            for (int i = 1; i < text.Length; i++)
            {
                if (char.IsUpper(text[i]))
                    if ((text[i - 1] != ' ' && !char.IsUpper(text[i - 1])) ||
                        (preserveAcronyms && char.IsUpper(text[i - 1]) &&
                         i < text.Length - 1 && !char.IsUpper(text[i + 1])))
                        newText.Append(' ');
                newText.Append(text[i]);
            }
            return newText.ToString();
        }
        private void comboBox1_Format(object sender, ListControlConvertEventArgs e)
        {
            e.Value = AddSpacesToSentence(e.ListItem.ToString(), true);
        }

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
                comboBox1.Format += comboBox1_Format;
                foreach (var option in _options.Where(o => !splitValues.Contains(o)).ToList())
                {
                    comboBox1.Items.Add(option);
                }
                foreach (var splitValue in splitValues.Where(sv => !string.IsNullOrWhiteSpace(sv)))
                {
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
