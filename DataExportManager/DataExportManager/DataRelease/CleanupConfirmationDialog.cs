using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.DataRelease;
using DataExportLibrary.ExtractionTime;
using ReusableUIComponents;


namespace DataExportManager.DataRelease
{
    /// <summary>
    /// Shown following the successful release of a project extraction.  This dialog will delete all temporary files, empty folders and remnants of old extractions not part of the final
    /// release.
    /// </summary>
    public partial class CleanupConfirmationDialog : Form
    {
        private readonly ReleaseEngine _releaser;
        readonly List<DirectoryInfo> DirectoriesToDelete = new List<DirectoryInfo>();

        public CleanupConfirmationDialog(ReleaseEngine releaser)
        {
            _releaser = releaser;

            InitializeComponent();
            
            if (releaser == null)
                return;

            if (!releaser.ReleaseSuccessful)
                throw new Exception("Cannot perform cleanup because ReleaseEngine reports that it was not successful");

            
            PopulateListViewWithFilesThatWillBeCleaned();
        }


        private void PopulateListViewWithFilesThatWillBeCleaned()
        {
            DirectoriesToDelete.Clear();
            listView1.Items.Clear();

            DirectoryInfo projectExtractionDirectory = new DirectoryInfo(_releaser.Project.ExtractionDirectory);

            foreach (ExtractionConfiguration config in _releaser.ConfigurationsReleased)
            {
                var directoryInfos = projectExtractionDirectory.GetDirectories().Where(d => ExtractionDirectory.IsOwnerOf(config, d));

                foreach (DirectoryInfo toCleanup in directoryInfos)
                    AddDirectoryToListViewRecursively(toCleanup, true);
            }

            foreach (ColumnHeader column in listView1.Columns)
                    column.Width = -2; //magical (apparently it resizes to max width of content or header)
        }

        private void AddDirectoryToListViewRecursively(DirectoryInfo toCleanup, bool isRoot)
        {
            //only add root folders to the delete queue
            if(isRoot)
                if(!DirectoriesToDelete.Any(dir=>dir.FullName.Equals(toCleanup.FullName))) //dont add the same folder twice
                    DirectoriesToDelete.Add(toCleanup);

            foreach (FileInfo file in toCleanup.EnumerateFiles())
            {
                ListViewItem item = new ListViewItem();
                item.Text = file.FullName;
                if(isRoot)
                {
                    item.ForeColor = Color.Red;
                    item.SubItems.Add("Unexpected Root Pollution");
                }
                else
                    item.SubItems.Add("Old File");
                item.Tag = file;
                listView1.Items.Add(item);
            }

            foreach (var dir in toCleanup.EnumerateDirectories())
                AddDirectoryToListViewRecursively(dir, false);

        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (listView1.Items.Count == 0)
            {
                this.Close();
                return;
            }

            int deletedsuccessfully = 0;

            foreach (ListViewItem item in listView1.Items)
            {
                FileInfo fileInfo = (FileInfo)item.Tag;
                lblAction.Text = "Action:" + "Deleting file " + fileInfo.FullName;
                
            TryAgain:
                try
                {
                    fileInfo.Delete();
                    deletedsuccessfully++;
                }
                catch (Exception exception)
                {
                    DialogResult dialogResult = MessageBox.Show("Could not delete file " + fileInfo.Name + " because of reason:"+exception.Message+", try again?","Delete Failed - Try Again?", MessageBoxButtons.AbortRetryIgnore);
                    if(dialogResult == DialogResult.Retry)
                        goto TryAgain;

                    if (dialogResult == DialogResult.Abort)
                    {
                        MessageBox.Show("You have chosen to abort, no more files will be deleted");
                        return;
                    }

                    //else it is ignore in which case just continue;
                }
            }

            MessageBox.Show("Sucesfully deleted " + deletedsuccessfully + " files, now deleting the (empty) folders");

            int foldersToDelete = DirectoriesToDelete.Count;
            
                //now delete root folders
                foreach (DirectoryInfo directoryInfo in DirectoriesToDelete)
                {
                    try
                    {
                        directoryInfo.Delete(true);
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show("Could not delete folder '"+directoryInfo.FullName + "':" +Environment.NewLine+Environment.NewLine + exception.Message );
                    }
                }
    
                MessageBox.Show("Deleted " + foldersToDelete + " Extraction folders");
         

            PopulateListViewWithFilesThatWillBeCleaned();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
