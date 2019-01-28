using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using CatalogueLibrary.Data;
using CatalogueManager.ItemActivation;
using CatalogueManager.SimpleControls;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using ReusableUIComponents;

namespace CatalogueManager.Validation
{
    /// <summary>
    /// Regular expressions are a great way of validating the content of your datasets.  For example you could have a regex pattern ^[MFU]$ which would force a cells contents to be either
    /// M, F or U with nothing else allowed.  Rather than having each data analyst type the same regular expression into the validation rules of each column you can create a StandardRegex.
    /// This StandardRegex will then be available as a validation rule for any column (See ValidationSetupForm).
    /// 
    /// <para>Because regular expressions can get pretty complicated both a concept name and a verbose description that explains what the pattern matches and what it won't match.  You can also 
    /// test your implementation by typing values into the 'Testing Box' and clicking Test.  For example if you typed in 'Male' with the above pattern it would fail validation because it
    /// is not either an M or a F or a U.  If your pattern was [MFU] then it would pass because it contains an M! </para>
    /// </summary>
    public partial class StandardRegexUI : StandardRegexUI_Design, ISaveableUI
    {
        private StandardRegex _standardRegex;

        public StandardRegexUI()
        {
            InitializeComponent();
        }
        public override void SetDatabaseObject(IActivateItems activator, StandardRegex databaseObject)
        {
            base.SetDatabaseObject(activator, databaseObject);

            _standardRegex = databaseObject;

            tbID.Text = databaseObject.ID.ToString();
            tbConceptName.Text = databaseObject.ConceptName;
            tbRegex.Text = databaseObject.Regex;
            tbDescription.Text = databaseObject.Description;
        }

        private void tbConceptName_TextChanged(object sender, EventArgs e)
        {
            tbConceptName.BackColor = Color.White;

            if(_standardRegex != null)
                if (string.IsNullOrWhiteSpace(tbConceptName.Text))
                    tbConceptName.BackColor = Color.Pink;
                else
                    _standardRegex.ConceptName = tbConceptName.Text;
        }

        private void tbRegex_TextChanged(object sender, EventArgs e)
        {
            tbRegex.BackColor = Color.White;

            if (_standardRegex != null)
                if (string.IsNullOrWhiteSpace(tbRegex.Text))
                    tbRegex.BackColor = Color.Pink;
                else
                {
                    try
                    {
                        Regex r = new Regex(tbRegex.Text);
                        _standardRegex.Regex = tbRegex.Text;
                        tbRegex.ForeColor = Color.Black;
                    }
                    catch (Exception)
                    {
                        tbRegex.ForeColor = Color.Red;
                    }
                }
        }

        private void tbDescription_TextChanged(object sender, EventArgs e)
        {
            if (_standardRegex != null)
                _standardRegex.Description = tbDescription.Text;
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbTesting.Text))
            {
                lblResultOfTest.Text = "The test text box is blank, null values will automatically pass validation (use a NotNull constraint to do null related evaluations)";
                lblResultOfTest.ForeColor = Color.Green;
            }
            else
                if (Regex.IsMatch(tbTesting.Text, _standardRegex.Regex))
            {
                lblResultOfTest.Text = "The text '" + tbTesting.Text + "' matches the Regex pattern '" + _standardRegex.Regex + "' meaning that the value will pass validation and not be flagged as a validation failure";
                lblResultOfTest.ForeColor = Color.Green;
            }
            else
            {
                lblResultOfTest.Text = "The text '" + tbTesting.Text + "' failed to match Regex pattern '" + _standardRegex.Regex + "' meaning that the value will fail validation and will be flagged as a validation failure";
                lblResultOfTest.ForeColor = Color.Red;
            }
        }

        public ObjectSaverButton GetObjectSaverButton()
        {
            return objectSaverButton1;
        }
    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<StandardRegexUI_Design, UserControl>))]
    public abstract class StandardRegexUI_Design : RDMPSingleDatabaseObjectControl<StandardRegex>
    {
    }
}
