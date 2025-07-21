using System.Data;
using System.Windows.Forms.DataVisualization.Charting;
using Rdmp.UI.TestsAndSetup.ServicePropogation;

namespace Rdmp.UI.CatalogueAnalysisUIs.Charts
{
    public partial class MultiPurposeChart: RDMPUserControl
    {
        private DataTable _dt;
        private SeriesChartType _chartType;
        private string _title;
        private string _seriesText;

        public MultiPurposeChart()
        {
            InitializeComponent();
        }

        public void Init(DataTable dt,SeriesChartType chartType = SeriesChartType.Pie, string title="", string seriesText="")
        {
            _dt = dt;
            _chartType = chartType;
            _title = title;
            _seriesText = seriesText;
            GenerateChart();
        }

        private void GenerateChart()
        {
            groupBox1.Text = _title;
            chart1.Visible = false;
            chart1.Series[0].ChartType = _chartType;
            //todo each chart has custom properties to make them look nice
            if (_dt is not null)
            {
                chart1.Series[0].XValueMember = _dt.Columns[1].ColumnName;
                chart1.Series[0].YValueMembers = _dt.Columns[0].ColumnName;
                if (!string.IsNullOrWhiteSpace(_seriesText)){
                    chart1.Series[0].Name = _seriesText;
                }
                chart1.DataSource = _dt;
                chart1.DataBind();
                chart1.Visible = true;
            }
        }

    }
}
