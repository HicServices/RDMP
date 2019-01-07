using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;

namespace ResearchDataManagementPlatform.Theme
{
    class ThemeExtender
    {
        private XDocument _xml;
        private const string Env = "Environment";

        public Color TextBoxBackground { get; set; }
        
        public Color ComboBoxBackground { get; set; }
        public Color ComboBoxText { get; set; }


        public ThemeExtender(byte[] bytes)
        {
            _xml = XDocument.Load(new StreamReader(new MemoryStream(bytes)));
            TextBoxBackground = ColorTranslatorFromHtml("CommonControls", "TextBoxBackground");
            
            ComboBoxBackground = ColorTranslatorFromHtml(Env, "ComboBoxBackground");
            ComboBoxText = ColorTranslatorFromHtml(Env, "ComboBoxText");
        }

        private Color ColorTranslatorFromHtml(string category, string name, bool foreground = false)
        {
            string color = null;

            XElement environmentElement = _xml.Root.Element("Theme").Elements("Category").FirstOrDefault(item => item.Attribute("Name").Value == category);

            if (environmentElement != null)
            {
                var colourElement = environmentElement.Elements("Color").FirstOrDefault(item => item.Attribute("Name").Value == name);

                if (colourElement != null)
                    color = colourElement.Element(foreground ? "Foreground" : "Background").Attribute("Source").Value;
            }

            if (color == null)
                return Color.Transparent;

            return ColorTranslator.FromHtml("#" + color);
        }

        public void ApplyTo(ToolStrip item)
        {
            foreach (var comboBox in item.Items.OfType<ToolStripComboBox>())
            {
                comboBox.ForeColor = ComboBoxText;
                comboBox.BackColor = ComboBoxBackground;
            }
        }
    }
}