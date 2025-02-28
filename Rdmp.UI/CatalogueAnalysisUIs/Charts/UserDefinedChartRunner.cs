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

namespace Rdmp.UI.CatalogueAnalysisUIs.Charts
{
    public partial class UserDefinedChartRunner : UserControl
    {
        private UserDefinedChart _chart;

        public UserDefinedChartRunner()
        {
            InitializeComponent();
        }

        public void Setup(UserDefinedChart chart)
        {
            _chart = chart;
            groupBox1.Text = _chart.Title;
            multiPurposeChart1.Init(_chart.GetResults(), (System.Windows.Forms.DataVisualization.Charting.SeriesChartType)_chart.ChartType, "", _chart.SeriesName);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _chart.Generate();
            Setup(_chart);
        }
    }
}
