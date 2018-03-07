using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Repositories;
using CatalogueManager.AggregationUIs.Advanced;
using CatalogueManager.Collections;
using CatalogueManager.ItemActivation;
using CatalogueManager.SimpleControls;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using DataLoadEngine.DatabaseManagement.EntityNaming;
using DataLoadEngine.LoadExecution.Components.Arguments;
using DataLoadEngine.LoadExecution.Components.Runtime;
using RDMPObjectVisualisation.DemandsInitializationUIs;
using ReusableLibraryCode.Checks;
using ReusableUIComponents;

namespace CatalogueManager.DataLoadUIs.LoadMetadataUIs.ProcessTasks
{

    /// <summary>
    /// Lets you view/edit a single data load module.  This is a pre-canned class e.g. FTPDownloader or a custom plugin you have written.  You should ensure
    /// that the Name field accurately describes (in plenty of detail) what the module/script is intended to do.  
    /// 
    /// These can be either:
    /// Attacher - Run the named C# class (which implements the interface IAttacher).  This only works in Mounting stage.  This usually results in records being loaded into the RAW bubble (e.g. AnySeparatorFileAttacher)
    /// DataProvider - Run the named C# class (which implements IDataProvider).  Normally this runs in GetFiles but really it can run on any Stage.  This usually results in files being created or modified (e.g. FTPDownloader)
    /// MutilateDataTable - Run the named C# class (which implements IMutilateDataTables).  Runs in any Adjust/PostLoad stage.  These are dangerous operations which operate pre-canned functionality directly
    /// on the DataTable being loaded e.g. resolving primary key collisions (which can result in significant data loss if you have not configured the correct primary keys on your dataset).
    /// 
    /// Each C# module based task has a collection of arguments which each have a description of how they change the behaviour of the module.  Make sure to click on each Argument in turn
    /// and set an appropriate value such that you understand ahead of time what the module will do when it is run.
    /// 
    /// The data load engine design (RAW,STAGING,LIVE) makes it quite difficult to corrupt your data without realising but you should still adopt best practice: Do as much data modification
    /// in the RAW bubble (i.e. not as a post load operation), only use modules you understand the function of and try to restrict the scope of your adjustment operations (it is usually better
    /// to write an extraction transform than to transform the data during load in case there is a mistake or a researcher wants uncorrupted original data).
    /// </summary>
    public partial class PluginProcessTaskUI : PluginProcessTaskUI_Design,ISaveableUI
    {
        private ArgumentCollection _argumentCollection;
        private Type _underlyingType;
        private ProcessTask _processTask;

        public PluginProcessTaskUI()
        {
            InitializeComponent();
            AssociatedCollection = RDMPCollection.DataLoad;
        }

        public override void SetDatabaseObject(IActivateItems activator, ProcessTask databaseObject)
        {
            base.SetDatabaseObject(activator, databaseObject);
            _processTask = databaseObject;

            if(_argumentCollection == null)
            {
                var repo = (CatalogueRepository) databaseObject.Repository;

                _argumentCollection = new ArgumentCollection();
                try
                {
                    _underlyingType = repo.MEF.GetTypeByNameFromAnyLoadedAssembly(databaseObject.GetClassNameWhoArgumentsAreFor());

                    if(_underlyingType == null)
                        throw new Exception("Could not find Type '" + databaseObject.GetClassNameWhoArgumentsAreFor() +"' for ProcessTask '" + _processTask.Name + "'");
                }
                catch (Exception e)
                {
                    ExceptionViewer.Show(e);
                    return;
                }

                _argumentCollection.Setup(repo, databaseObject, _underlyingType);

                _argumentCollection.Dock = DockStyle.Fill;
                pArguments.Controls.Add(_argumentCollection);
            }

            CheckComponent();

            tbName.Text = databaseObject.Name;
            
            loadStageIconUI1.Setup(_activator.CoreIconProvider,_processTask.LoadStage);

            objectSaverButton1.SetupFor(databaseObject,_activator.RefreshBus);
        }

        private void CheckComponent()
        {
            try
            {
                var factory = new RuntimeTaskFactory(_activator.RepositoryLocator.CatalogueRepository);

                var lmd = _processTask.LoadMetadata;
                var argsDictionary = new LoadArgsDictionary(lmd, new HICDatabaseConfiguration(lmd).DeployInfo);
                var mefTask = (IMEFRuntimeTask) factory.Create(_processTask, argsDictionary.LoadArgs[_processTask.LoadStage]);
            
                ragSmiley1.StartChecking(mefTask.MEFPluginClassInstance);
            }
            catch (Exception e)
            {
                ragSmiley1.Fatal(e);
            }

        }

        public ObjectSaverButton GetObjectSaverButton()
        {
            return objectSaverButton1;
        }

        private void tbName_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbName.Text))
            {
                tbName.Text = "No Name";
                tbName.SelectAll();
            }

            _processTask.Name = tbName.Text;
        }

        private void btnCheckAgain_Click(object sender, EventArgs e)
        {
            CheckComponent();
        }
    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<PluginProcessTaskUI_Design, UserControl>))]
    public abstract class PluginProcessTaskUI_Design:RDMPSingleDatabaseObjectControl<ProcessTask>
    {
    }
}
