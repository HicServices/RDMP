using System.Windows.Forms;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.Data.Cohort;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.Menus;
using CohortManager.CommandExecution.AtomicCommands;
using DataExportLibrary.Data;
using DataExportManager.CommandExecution.AtomicCommands;
using DataExportManager.CommandExecution.AtomicCommands.CohortCreationCommands;

namespace CohortManager.Menus
{

    [System.ComponentModel.DesignerCategory("")]
    class CohortIdentificationConfigurationMenu :RDMPContextMenuStrip
    {
        private CohortIdentificationConfiguration _cic;
        private IAtomicCommandWithTarget _executeAndImportCommand;
        private IAtomicCommandWithTarget _executeCommandClone;

        public CohortIdentificationConfigurationMenu(RDMPContextMenuStripArgs args, CohortIdentificationConfiguration cic): base(args, cic)
        {
            _cic = cic;

            Items.Add("View SQL", _activator.CoreIconProvider.GetImage(RDMPConcept.SQL), (s, e) => _activator.ActivateViewCohortIdentificationConfigurationSql(this, cic));
                
            Items.Add(new ToolStripSeparator());

            _executeAndImportCommand = new ExecuteCommandCreateNewCohortByExecutingACohortIdentificationConfiguration(_activator).SetTarget(cic);
            
            Add(_executeAndImportCommand);
            
            //associate with project
            Add(new ExecuteCommandAssociateCohortIdentificationConfigurationWithProject(_activator).SetTarget(cic));
            
            Items.Add(new ToolStripSeparator());

            _executeCommandClone = new ExecuteCommandCloneCohortIdentificationConfiguration(_activator).SetTarget(cic);
            Add(_executeCommandClone);

            Add(new ExecuteCommandFreezeCohortIdentificationConfiguration(_activator, cic, !cic.Frozen));
            
            Items.Add(new ToolStripSeparator());

            Add(new ExecuteCommandCreateNewCohortIdentificationConfiguration(_activator));
        }

        public CohortIdentificationConfigurationMenu(RDMPContextMenuStripArgs args, ProjectCohortIdentificationConfigurationAssociation association) : this(args,association.CohortIdentificationConfiguration)
        {
            _executeAndImportCommand.SetTarget(association.Project);
            _executeCommandClone.SetTarget(association.Project);
        }
        
    }
}
