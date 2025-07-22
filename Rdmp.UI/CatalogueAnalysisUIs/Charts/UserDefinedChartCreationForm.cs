using Rdmp.Core.CatalogueAnalysisTools.Data;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Repositories;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Rdmp.UI.CatalogueAnalysisUIs.Charts
{
    public partial class UserDefinedChartCreationForm : Form
    {
        private DQERepository _DQERepository;
        private IBasicActivateItems _activator;
        private Catalogue _catalogue;
        private bool _isEdit = false;
        private UserDefinedChart _chart;


        public UserDefinedChartCreationForm()
        {
            InitializeComponent();
        }

        public void Setup(IBasicActivateItems activator, DQERepository repository, UserDefinedChart chart)
        {
            _isEdit = true;
            _activator = activator;
            _DQERepository = repository;
            _chart = chart;
            comboBox1.Items.AddRange(Enum.GetNames(typeof(SeriesChartType)));
            textBox1.Text = _chart.Title;
            textBox2.Text = _chart.SeriesName;
            comboBox1.SelectedIndex = _chart.ChartType;
            textBox3.Text = _chart.QueryString;
            button1.Text = "Update";
        }

        public void Setup(IBasicActivateItems activator, DQERepository repository, Catalogue catalogue)
        {
            _activator = activator;
            _DQERepository = repository;
            _catalogue = catalogue;
            comboBox1.Items.AddRange(Enum.GetNames(typeof(SeriesChartType)));
            comboBox1.Items.Add("Table");
        }


        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            label1.Text = "";
            if (_DQERepository is null) return;
            if (string.IsNullOrWhiteSpace(textBox3.Text))//query
            {
                label1.Text = "No query string";
                return;
            }
            if (comboBox1.SelectedIndex == -1)//no chart selected
            {
                label1.Text = "No chart type";

                return;
            }
            if (!_isEdit)
            {
                var udc = new UserDefinedChart(_DQERepository, _catalogue, textBox3.Text, comboBox1.SelectedIndex, textBox1.Text, textBox2.Text);
                udc.SaveToDatabase();
                //todo background this and alert the user
                udc.Generate();
            }
            else
            {
                _chart.Title = textBox1.Text;
                _chart.SeriesName = textBox2.Text;
                _chart.ChartType = comboBox1.SelectedIndex;
                _chart.QueryString = textBox3.Text;
                _chart.SaveToDatabase();
            }
                this.Close();
        }
    }
}
