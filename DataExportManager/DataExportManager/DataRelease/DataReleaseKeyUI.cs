using System;
using System.Windows.Forms;

namespace DataExportManager.DataRelease
{
    /// <summary>
    /// Provides a key that describes the state of a given dataset in ConfigurationReleasePotentialUI. The statuses are:
    ///  
    /// <para>'Never been successfully extracted' - The RDMP has no record of an extraction having taken place on the dataset (including a failed one).</para>
    /// 
    /// <para>'Missing extract or metadata' - The RDMP has a record of the dataset being extracted but either the extracted data file or accompanying metadata file was not found in the correct
    /// location on disk (either it was moved or the extraction crashed halfway through - see LogViewerForm).</para>
    /// 
    /// <para>'Extraction SQL used to generate extract is out of sync with current Configuration' - Because project extractions can take some time to do it is possible that another data analyst
    /// (or you without realising it) makes a change to a dataset in the project which has already been extracted (e.g. selecting an additional column for extraction).  If this happens your
    /// extracted file will be wrong (it no longer reflects the current configuration).  In such a situation you will see this icon.  Right clicking the dataset will let you see the 
    /// difference(s) between the current configuration and the extracted data file.  To resolve this you must either rectify the configuration or re-extract the file.</para>
    /// 
    /// <para>'Extracted file was for a different cohort than the current setting' - Similar to the last one above except that the change to the configuration is to switch to a different cohort.
    /// This is the worst case scenario for release error where you supply a file to a researcher when the file doesn't even relate to the cohort he is asking for (or has ethics approval 
    /// for).  Imagine the trouble you would bring down if you release 1,000,000 anonymised patient records to a project that has ethics approval for 10 specific patients.</para>
    /// 
    /// <para>'File exists and is current' - Data file and metadata file are releasable and reflect the current configuration</para>
    /// 
    /// <para>'Exception occurred while assessing releaseability' - Something went wrong while the RDMP was trying to figure out if the files and current configuration match.  Right click the
    /// dataset to view the Exception.</para>
    /// 
    /// <para>'Column Level Differences vs Catalogue' - Not an error just a warning that you have changed the definition of columns for your extract (overridden the catalogue version of one or
    /// more columns - See ConfigureDatasetUI for how to do this).  Alternatively this can occur if someone has edited the master Catalogue implementation of a transform which is part
    /// of your configuration (Configuration is outdated vs the catalogue).  You should evaluate the differences and make sure they are intended before doing a release (by right clicking
    /// the dataset and viewing differences).</para>
    /// 
    /// </summary>
    public partial class DataReleaseKeyUI : UserControl
    {
        public DataReleaseKeyUI()
        {
            InitializeComponent();
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
