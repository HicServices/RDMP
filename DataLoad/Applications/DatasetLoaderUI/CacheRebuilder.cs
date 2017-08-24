using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using CachingEngine.Factories;
using CachingEngine.Requests;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cache;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using ReusableLibraryCode.Progress;
using ReusableUIComponents;

namespace DatasetLoaderUI
{
    /// <summary>
    /// Lists all the ICacheRebuilder classes in any plugins you have written.  These classes are intended to take Archived zip files of completed DLE runs and reverse engineer cache entries
    /// in the original cache (to effectively unwind a data load from the files perspective).  
    /// 
    /// You are 100% on your own when it comes to this I'm afraid, try asking whoever wrote the selected ICacheRebuilder if you have problems
    /// </summary>
    public partial class CacheRebuilder : RDMPForm
    {
        private CancellationTokenSource _cts = null;

        public CacheRebuilder()
        {
            InitializeComponent();

            
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (VisualStudioDesignMode)
                return;

            var mef = RepositoryLocator.CatalogueRepository.MEF;
            FormsHelper.SetupAutoCompleteForTypes(ddRebuilderClass, mef.GetTypes<ICacheRebuilder>());

        }

        private void btnAddFiles_Click(object sender, EventArgs e)
        {
            if (archiveFilePickerDialog.ShowDialog() != DialogResult.OK)
                return;

            lbArchiveFileList.Items.AddRange(
                archiveFilePickerDialog.FileNames.ToArray()
            );
        }

        private async void btnStartRebuild_Click(object sender, EventArgs e)
        {
            if (_cts != null)
                return; // re-entrancy
            
            using (_cts = new CancellationTokenSource())
            {

                var factory = new CacheRebuilderFactory(RepositoryLocator.CatalogueRepository);
                var rebuilder = factory.Create(ddRebuilderClass.SelectedItem as string);
                ToggleUI(true);
                try
                {
                    await rebuilder.RebuildCacheFromArchiveFiles(lbArchiveFileList.Items.Cast<string>().ToArray(), tbDestinationPath.Text, progressUI, _cts.Token);
                }
                catch (OperationCanceledException)
                {
                    progressUI.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Operation cancelled successfully"));
                }
                catch (AggregateException ex)
                {
                    ExceptionViewer.Show(ex);
                }

                ToggleUI(false);
            }

            _cts = null;
        }

        private void ToggleUI(bool isRebuildInProgress)
        {
            ddRebuilderClass.Enabled = !isRebuildInProgress;
            tbDestinationPath.Enabled = !isRebuildInProgress;
            btnAddFiles.Enabled = !isRebuildInProgress;
            btnRemoveFiles.Enabled = !isRebuildInProgress;
            btnStartRebuild.Enabled = !isRebuildInProgress;
            btnStopRebuild.Enabled = isRebuildInProgress;
        }

        private void btnStopRebuild_Click(object sender, EventArgs e)
        {
            _cts.Cancel();
        }

        private void btnShowDestinationPath_Click(object sender, EventArgs e)
        {
            Process.Start(tbDestinationPath.Text);
        }
    }
}
