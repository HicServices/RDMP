using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using MapsDirectlyToDatabaseTable;
using ReusableUIComponents;

namespace CatalogueManager.LocationsMenu.LocationAdjustment
{
    /// <summary>
    /// The RDMP is designed to track and curate research datasets over a long period of time (e.g. 20-30 years).  During this time you are likely to configure many SQL scripts, amass many
    /// supporting documents, set project extraction directories etc.  This is usually done through either mapped network paths (e.g. Z:\MyDataFiles\)  or named servers 
    /// (e.g. \\MyServer\MyData) Now sometimes your network team will decide to move all your files to a new server or remap a drive mapping for some reason.  This poses a problem for the 
    /// RDMP since there are many different cells in the database in different tables which store references to these files.
    /// 
    /// This window lets you deal with the above problem by performing bulk rename operations across all records in the entire RDMP. In the main form you will see a list of all the objects 
    /// in the database which the software thinks is the location of a file.  Before you start you might want to make a backup of your Data Catalogue and Data Export databases.  You can 
    /// review the fields content and format a suitable Find/Replace or just manually edit each Text box (although that could get tedious!).  Once you have executed a Find/Replace you should
    /// review the new values to make sure they exist/are correct before hitting Save All
    /// </summary>
    public partial class LocationsAdjuster : RDMPForm
    {
        readonly Dictionary<PropertyInfo, IMapsDirectlyToDatabaseTable[]> adjustablePropertiesDictionary;

        public LocationsAdjuster()
        {
            adjustablePropertiesDictionary = new Dictionary<PropertyInfo, IMapsDirectlyToDatabaseTable[]>();
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            
            if(VisualStudioDesignMode)
                return;
            
            List<Exception> ex;

            //get all saveable IMapsDirectlyToDatabaseTable objects
            IEnumerable<Type> mapsDirectlyToDatabaseTableTypes = RepositoryLocator.CatalogueRepository.MEF.GetAllTypesFromAllKnownAssemblies(out ex)
                .Where(
                t => typeof(IMapsDirectlyToDatabaseTable).IsAssignableFrom(t)
                    &&
                    typeof(ISaveable).IsAssignableFrom(t)
                );

            foreach (Type t in mapsDirectlyToDatabaseTableTypes)
            {
                PropertyInfo[] adjustablePropertyInfos = t.GetProperties().Where(p => Attribute.IsDefined(p, typeof(AdjustableLocation))).ToArray();

                if (t.IsAbstract || t.IsInterface)
                    continue;
                
                if (!adjustablePropertyInfos.Any())
                    continue;

                bool isCatalogue = t.Assembly.GetName().Name.Equals("CatalogueLibrary");
                bool isDataExport = t.Assembly.GetName().Name.Equals("DataExportLibrary");

                if (!isCatalogue && !isDataExport)
                    throw new NotSupportedException("Type " + t.Name + " has a property marked with [AdjustableLocation] but is not from cataloguelibrary or data export library");

                IMapsDirectlyToDatabaseTable[] allObjects;

                
                if (isCatalogue)
                    allObjects = RepositoryLocator.CatalogueRepository.GetAllObjects(t).ToArray();
                else
                    if (RepositoryLocator.DataExportRepository == null)
                        //user has not defined a connection to the data export manager database so don't attempt to allow refactoring
                        continue;
                    else
                        allObjects = RepositoryLocator.DataExportRepository.GetAllObjects(t).ToArray();

                //if there are not any objects dont offer refactoring
                if (!allObjects.Any())
                    continue;

                //there are objects and adjustable location properties
                foreach (var adjustableProperty in adjustablePropertyInfos)
                    adjustablePropertiesDictionary.Add(adjustableProperty, allObjects);
            }

            base.OnLoad(e);
        }

        private void btnFindAndReplaceInAll_Click(object sender, EventArgs e)
        {
            if (
                MessageBox.Show(
                    "Are you sure you want to find all values in the above control with '" + tbToFind.Text +
                    "' and replace them with '" + tbToReplaceWith.Text + "'? ",
                    "Are you sure? This can break your Catalogue badly if you have made a mistake!", MessageBoxButtons.YesNoCancel) ==
                DialogResult.Yes)
            {
                try
                {
                    int totalReplacements = 0;

                    foreach (LocationAdjustablePropertyUI p in tableLayoutPanel1.Controls)
                        totalReplacements += p.FindAndReplace(tbToFind.Text, tbToReplaceWith.Text);

                    if (totalReplacements == 0)
                        MessageBox.Show("Did not find the text anywhere in any of the objects");
                    else
                        MessageBox.Show("Made " + totalReplacements +" replacements, you should review these then hit SaveAll");
                }
                catch (Exception exception)
                {
                    ExceptionViewer.Show(exception);
                }
            }
        }

        private void btnSaveAll_Click(object sender, EventArgs e)
        {

            foreach (LocationAdjustablePropertyUI p in tableLayoutPanel1.Controls)
                p.SaveAll();

        }

        private void LocationsAdjuster_Load(object sender, EventArgs e)
        {

            tableLayoutPanel1.RowCount = adjustablePropertiesDictionary.Count;

            int row = 0;
            try
            {
                foreach (KeyValuePair<PropertyInfo, IMapsDirectlyToDatabaseTable[]> kvp in adjustablePropertiesDictionary)
                {
                    if(Process.GetCurrentProcess().HandleCount > 9000)
                        throw new Exception("There are too many objects in your database to display on the screen at once!");

                    LocationAdjustablePropertyUI ui = new LocationAdjustablePropertyUI(kvp.Key, kvp.Value);
                    ui.Dock = DockStyle.Fill;
                    tableLayoutPanel1.Controls.Add(ui, 1, row);
                    row++;
                }
            }
            catch (Exception exception)
            {
                if (exception.Message.Contains("Window Handle"))
                    MessageBox.Show("Windows is complaining about the number of controls being created, possibly you have too many objects in  your database to be displayed");

                ExceptionViewer.Show(exception );
            }
        }
    }
}
