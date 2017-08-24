using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.DataHelper;

namespace CatalogueManager.AggregationUIs
{
    /// <summary>
    /// One Dimension (Group By Column) can be set to be a 'Continuous Date Axis'.  This must be a DateTime or Date field or a transform that yields a Date/DateTime.  Normally with SQL
    /// when you include a Date in a GroupBy it will group it by unique value (just like any other field), most SQL users will get around this by using a function such as Year(MyDateCol) 
    /// to produce Aggregate of records per year.  However this approach will not fill in years where no date exists.
    /// 
    /// Setting an AggregateContinuousDateAxis will generate a continuous record set for the date field specified even when there are no records.  For example you can set up an axis that
    /// goes from 2001-01-01 to 2016-01-01 in increments of 1 month without having to worry about gaps in the axis or outlier dates (e.g. freaky dates like 1900-01-01).
    /// </summary>
    public partial class AggregateContinuousDateAxisUI : UserControl
    {
        private AggregateDimension _dimension;
        private AggregateContinuousDateAxis _axis;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public AggregateDimension Dimension
        {
            get { return _dimension; }
            set
            {
                _dimension = value;

                if (value == null)
                {
                    groupBox1.Enabled = false;
                    return;
                }
                else
                    groupBox1.Enabled = true;

                _axis = value.AggregateContinuousDateAxis;

                UpdateFormStateToMatchAxisState();
            
            }
        }

        private bool updating = false;

        public event Action AxisSaved;

        private void UpdateFormStateToMatchAxisState()
        {
            updating = true;
            if (_axis == null)
            {
                tbEndDate.Visible = false;
                tbStartDate.Visible = false;
                label1.Visible = false;
                label2.Visible = false;
                label3.Visible = false;
                ddIncrement.Visible = false;
            }
            else
            {
                tbEndDate.Visible = true;
                tbStartDate.Visible = true;
                label1.Visible = true;
                label2.Visible = true;
                label3.Visible = true;
                ddIncrement.Visible = true;

                tbStartDate.Text = _axis.StartDate.ToString();
                tbEndDate.Text = _axis.EndDate.ToString();
                ddIncrement.SelectedItem = _axis.AxisIncrement;

            }

            updating = false;
        }

        public AggregateContinuousDateAxisUI()
        {
            InitializeComponent();
            ddIncrement.DataSource = new object[]{AxisIncrement.Month,AxisIncrement.Quarter,AxisIncrement.Year}; //dont offer day, that was a terrible idea
        }


        private void ddIncrement_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (updating || _axis == null)
                return;

            _axis.AxisIncrement = (AxisIncrement) ddIncrement.SelectedValue;
            _axis.SaveToDatabase();

            AxisSaved();
        }
        
        private void tbDates_TextChanged(object sender, EventArgs e)
        {
            if (updating || _axis == null)
                return;

            TextBox s = (TextBox) sender;

            try
            {
                //if user enters a date then put 
                DateTime dt = DateTime.Parse(s.Text);

                updating = true;
                s.Text = "'" + dt.ToString("yyyy-MM-dd") + "'";
                updating = false;
            }
            catch (Exception)
            {
                
            }

            if(s == tbStartDate)
                _axis.StartDate = s.Text;
            else
                _axis.EndDate = s.Text;

            try
            {
                RDMPQuerySyntaxHelper.ParityCheckCharacterPairs(new[] {'(', '\''}, new[] {')', '\''}, s.Text);
                s.ForeColor = Color.Black;

                _axis.SaveToDatabase();
                AxisSaved();
            }
            catch (SyntaxErrorException)
            {
                s.ForeColor = Color.Red;
            }

        }

     
    }
}
