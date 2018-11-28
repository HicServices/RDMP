using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Dashboarding;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.CommandExecution.AtomicCommands.UIFactory;
using CatalogueManager.ItemActivation;
using CatalogueManager.ItemActivation.Emphasis;
using CatalogueManager.Refreshing;
using CatalogueManager.SimpleDialogs.Reports;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using MapsDirectlyToDatabaseTable;
using ResearchDataManagementPlatform.WindowManagement.ExtenderFunctionality;
using ReusableUIComponents;

namespace ResearchDataManagementPlatform.WindowManagement.ContentWindowTracking.Persistence
{
    /// <summary>
    /// A Document Tab that hosts an RDMPSingleDatabaseObjectControl T, the control knows how to save itself to the persistence settings file for the user ensuring that when they next open the
    /// software the Tab can be reloaded and displayed.  Persistance involves storing this Tab type, the Control type being hosted by the Tab (a RDMPSingleDatabaseObjectControl) and the object
    /// ID , object Type and Repository (DataExport or Catalogue) of the T object currently held in the RDMPSingleDatabaseObjectControl.
    /// </summary>
    [System.ComponentModel.DesignerCategory("")]
    [TechnicalUI]
    public class PersistableSingleDatabaseObjectDockContent : RDMPSingleControlTab
    {
        private readonly Control _control;
        public IMapsDirectlyToDatabaseTable DatabaseObject { get; private set; }

        public const string Prefix = "RDMPSingleDatabaseObjectControl";

        public PersistableSingleDatabaseObjectDockContent(IRDMPSingleDatabaseObjectControl control, IMapsDirectlyToDatabaseTable databaseObject,RefreshBus refreshBus):base(refreshBus)
        {
            _control = (Control)control;
            
            DatabaseObject = databaseObject;
            TabText = "Loading...";
        }

        protected override string GetPersistString()
        {
            const char s = PersistStringHelper.Separator;
            return Prefix + s + _control.GetType().FullName + s + DatabaseObject.Repository.GetType().FullName + s + DatabaseObject.GetType().FullName + s + DatabaseObject.ID;
        }

        public override Control GetControl()
        {
            return _control;
        }

        public override void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
        {
            var newTabName = ((IRDMPSingleDatabaseObjectControl) _control).GetTabName();
            var floatWindow = ParentForm as CustomFloatWindow;

            if (floatWindow != null)
                floatWindow.Text = newTabName;

            TabText = newTabName;
        }

        public override void HandleUserRequestingTabRefresh(IActivateItems activator)
        {
            var cmd = new ExecuteCommandRefreshObject(activator, DatabaseObject as DatabaseEntity);

            if (!cmd.IsImpossible)
                cmd.Execute();
        }

        public override void HandleUserRequestingEmphasis(IActivateItems activator)
        {
            activator.RequestItemEmphasis(this,new EmphasiseRequest(DatabaseObject));
        }
    }
}
