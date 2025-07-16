using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Rdmp.Core.CatalogueAnalysisTools.Data;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Repositories;

namespace Rdmp.UI.CatalogueAnalysisUIs.Charts
{
    public partial class UserDefinedChartRunner : UserControl
    {
        private UserDefinedChart _chart;
        private IBasicActivateItems _activator;
        private DQERepository _dqeRepository;

        public UserDefinedChartRunner()
        {
            InitializeComponent();
        }

        public void Setup(IBasicActivateItems activator, DQERepository dqeRepository, UserDefinedChart chart)
        {
            _activator = activator;
            _dqeRepository = dqeRepository;
            _chart = chart;
            groupBox1.Text = _chart.Title;
            multiPurposeChart1.Init(_chart.GetResults(), (System.Windows.Forms.DataVisualization.Charting.SeriesChartType)_chart.ChartType, "", _chart.SeriesName);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _chart.Generate();
            Setup(_activator,_dqeRepository,_chart);
        }

        private void multiPurposeChart1_Load(object sender, EventArgs e)
        {

        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            var dialog = new UserDefinedChartCreationForm();
            dialog.Setup(_activator, _dqeRepository, _chart);
            dialog.Show();
        }
    }
}
