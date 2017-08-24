using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using MapsDirectlyToDatabaseTable;

namespace CatalogueManager.LocationsMenu.LocationAdjustment
{
    /// <summary>
    /// Used in LocationAdjuster which is a tool for performing mass find/replace on file paths in the RDMP database across all tables.  For full details see LocationAdjuster documentation.
    /// This component is a collection of all the cells in a given column which has been marked as adjustable (that means it is likely to contain lots of file paths the user may wish to 
    /// do Find/Replace on).  For each object/row which has a value in this column, a LocationAdjustableObjectUI control will appear (See LocationAdjustableObjectUI) for documentation.
    /// 
    /// For example the field in SupportingDocument table/class has a field/property URL which is marked with [AdjustableLocation] this mean that if the user has 3 records 'Z:\Docs\Doc1.doc,
    /// 'Z:\Docs\Doc2.doc' and 'Z:\Docs\Doc3.doc' and his network administrator remaps Z: to X: on all the machines on the network then the data analyst can use this control to bulk rename Z:\
    /// to X:\ 
    /// </summary>
    public partial class LocationAdjustablePropertyUI : UserControl
    {
        public LocationAdjustablePropertyUI(PropertyInfo property, IMapsDirectlyToDatabaseTable[] objects)
        {
            
            InitializeComponent();

            if (property == null)
                return;

            Type type = objects.Select(o => o.GetType()).Distinct().Single();
            lblProperty.Text = "Property:" + property.Name + " (" + type.Name + ")";

            int requiredHeight = 0;
            int row = 0;


            foreach (IMapsDirectlyToDatabaseTable o in objects)
            {
                object val = property.GetValue(o);

                //its null so no refactoring will be happening
                if( val == null || string.IsNullOrWhiteSpace(val.ToString()))
                    continue;

                //its probably not a file path
                if(!val.ToString().Contains('\\') && !val.ToString().Contains('/'))
                    continue;
                
                var ui = new LocationAdjustableObjectUI(property, o);
                ui.Dock = DockStyle.Fill;
                
                this.Width = Math.Max(this.Width,ui.PreferredSize.Width+20);
                requiredHeight += ui.PreferredSize.Height + 7;
                tableLayoutPanel1.Controls.Add(ui,0,row);

                row++;

            }
            tableLayoutPanel1.RowCount = row;
            this.Height = requiredHeight + 50;
        }

        public int FindAndReplace(string toFind, string toReplaceWith)
        {
            int runningTotal = 0;

            foreach (LocationAdjustableObjectUI c in tableLayoutPanel1.Controls)
                runningTotal += c.FindAndReplace(toFind, toReplaceWith);

            return runningTotal;
        }

        public void SaveAll()
        {
            foreach (LocationAdjustableObjectUI c in tableLayoutPanel1.Controls)
                c.Save();
        }
    }
}
