using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dashboard.CatalogueSummary.DataQualityReporting.SubComponents
{
    /// <summary>
    /// All validation rules configured in the RDMP have an associated 'Consequence', this is like a severity level.  The lowest is 'Missing' and indicates that the failure of the 
    /// validation rule means that an expected value is not present.  The worst is 'Invalidates Row' which indicates that the validation failure is so serious that the entire row
    /// is useless (e.g. a hospital admissions record with no patient identifier making it unlinkable).  See SecondaryConstraintUI for more information on how validation rules are 
    /// interpreted.
    /// 
    /// This control documents which colours are used to render each of these consequences in ColumnStatesChart. 
    /// </summary>
    public partial class ConsequenceKey : UserControl
    {
        public ConsequenceKey()
        {
            InitializeComponent();
            lblCorrect.BackColor = ConsequenceBar.CorrectColor;
            lblMissing.BackColor = ConsequenceBar.MissingColor;
            lblWrong.BackColor = ConsequenceBar.WrongColor;
            lblInvalid.BackColor = ConsequenceBar.InvalidColor;
            lblHasValue.BackColor = ConsequenceBar.HasValuesColor;
            lblIsNull.BackColor = ConsequenceBar.IsNullColor;

        }

    }
}
