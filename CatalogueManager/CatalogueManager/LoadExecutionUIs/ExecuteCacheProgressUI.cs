using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cache;
using CatalogueManager.Collections;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.ItemActivation;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using RDMPAutomationService.Options;
using RDMPAutomationService.Options.Abstracts;
using ReusableUIComponents;

namespace CatalogueManager.LoadExecutionUIs
{
    /// <summary>
    /// Allows you to execute a Caching pipeline for a series of days.  For example this might download files from a web service by date and store them in a cache directory
    /// for later loading.  Caching is independent of data loading and only required if you have a long running fetch process which is time based and not suitable for
    /// execution as part of the load (due to the length of time it takes or the volatility of the load or just because you want to decouple the two processes).
    /// </summary>
    public partial class ExecuteCacheProgressUI : CachingEngineUI_Design
    {
        private ICacheProgress _cacheProgress;
        
        public ExecuteCacheProgressUI()
        {
            InitializeComponent();
            AssociatedCollection = RDMPCollection.DataLoad;
            checkAndExecuteUI1.CommandGetter += CommandGetter;
        }

        private RDMPCommandLineOptions CommandGetter(CommandLineActivity commandLineActivity)
        {
            return new CacheOptions()
            {
                CacheProgress = _cacheProgress.ID,
                Command = commandLineActivity,
                RetryMode = cbFailures.Checked
            };
        }

        public override void SetDatabaseObject(IActivateItems activator, CacheProgress databaseObject)
        {
            base.SetDatabaseObject(activator, databaseObject);

            _cacheProgress = databaseObject;

            Add(new ExecuteCommandEditCacheProgress(activator, databaseObject),"Edit");

            bool failures = _cacheProgress.CacheFetchFailures.Any(f => f.ResolvedOn == null);
            cbFailures.Enabled = failures;
            Add(new ExecuteCommandShowCacheFetchFailures(activator,databaseObject));
            
            checkAndExecuteUI1.SetItemActivator(activator);
        }
        public override void ConsultAboutClosing(object sender, FormClosingEventArgs e)
        {
            base.ConsultAboutClosing(sender, e);
            checkAndExecuteUI1.ConsultAboutClosing(sender, e);
        }
    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<CachingEngineUI_Design, UserControl>))]
    public abstract class CachingEngineUI_Design : RDMPSingleDatabaseObjectControl<CacheProgress>
    {
        
    }
}

