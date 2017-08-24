using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;

namespace CatalogueManager.LocationsMenu.LocationAdjustment
{
    /// <summary>
    /// Used in LocationAdjuster which is a tool for performing mass find/replace on file paths in the RDMP database across all tables.  For full details see LocationAdjuster documentation.
    /// This control reflects a single cell of a single record which the system has determined contains a reference to a physical location e.g. 'Z:\Documentation\MyDoc.docx'.  You can manually
    /// change the value of it using this control or use LocationAdjuster to do bulk operations across all database records/tables in the RDMP database.
    /// </summary>
    public partial class LocationAdjustableObjectUI : UserControl
    {
        private readonly PropertyInfo _property;
        private readonly IMapsDirectlyToDatabaseTable _o;

        bool _setupComplete;

        public LocationAdjustableObjectUI(PropertyInfo property, IMapsDirectlyToDatabaseTable o)
        {
            _property = property;
            _o = o;
            InitializeComponent();

            if (property == null)
                return;

            if (o is ISaveable == false)
                throw new NotSupportedException("Cannot adjust locations on objects which are not ISaveable");

            if (property.PropertyType != typeof(string) && property.PropertyType != typeof(Uri))
                throw new NotSupportedException("[AdjustableLocation] must be of type string or Uri but it was " + property.PropertyType);
            
            lblObjectNameAndID.Text = o + "(ID="+ o.ID+")";

            var objectValue = property.GetValue(_o);
            tbPropertyValue.Text = objectValue == null?"": objectValue.ToString();

            _setupComplete = true;
        }

        private void tbPropertyValue_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if(string.IsNullOrWhiteSpace(tbPropertyValue.Text))
                    _property.SetValue(_o,null);
                else
                if(_property.PropertyType == typeof(Uri))
                    _property.SetValue(_o,new Uri(tbPropertyValue.Text));
                else
                    _property.SetValue(_o,tbPropertyValue.Text);
                 
                if(_setupComplete)
                    btnSave.Enabled = true;

                lblObjectNameAndID.ForeColor = Color.Black;


            }
            catch (Exception)
            {
                lblObjectNameAndID.ForeColor = Color.Red;
            }
        }
        
        private void btnSave_Click(object sender, EventArgs e)
        {
            Save();
        }


        public int FindAndReplace(string toFind, string toReplaceWith)
        {
            toFind = Regex.Escape(toFind);
            int count = new Regex(toFind,RegexOptions.IgnoreCase).Matches(tbPropertyValue.Text).Count;

            if(count > 1)
                throw new Exception("Users string '" + toFind +"' appears multiple times object " + _o + " so we are refusing to find/replace just to be on the safe side");

            tbPropertyValue.Text = Regex.Replace(tbPropertyValue.Text,toFind, toReplaceWith,RegexOptions.IgnoreCase);

            return count;
        }

        public void Save()
        {
            btnSave.Enabled = false;
            ((ISaveable)_o).SaveToDatabase();
        }
    }
}
