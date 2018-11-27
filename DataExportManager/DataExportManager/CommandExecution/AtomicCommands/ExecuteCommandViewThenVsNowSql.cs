using System;
using System.Drawing;
using System.Windows.Forms;
using CatalogueLibrary.QueryBuilding;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using DataExportLibrary.Data.LinkCreators;
using DataExportLibrary.DataRelease.Potential;
using DataExportLibrary.Interfaces.Data.DataTables;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents;
using ReusableUIComponents.SqlDialogs;

namespace DataExportManager.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandViewThenVsNowSql : BasicUICommandExecution, IAtomicCommand
    {
        private FlatFileReleasePotential _releasePotential;

        public ExecuteCommandViewThenVsNowSql(IActivateItems activator, SelectedDataSets selectedDataSet):base(activator)
        {
            try
            {
                var rp = new FlatFileReleasePotential(Activator.RepositoryLocator, selectedDataSet);

                rp.Check(new IgnoreAllErrorsCheckNotifier());

                if (string.IsNullOrWhiteSpace(rp.SqlCurrentConfiguration))
                    SetImpossible("Could not generate Sql for dataset");
                else
                if(string.IsNullOrWhiteSpace(rp.SqlExtracted))
                    SetImpossible("Dataset has never been extracted");
                else
                if(rp.SqlCurrentConfiguration == rp.SqlExtracted)
                    SetImpossible("No differences");

                _releasePotential = rp;
            }
            catch (Exception)
            {
                SetImpossible("Could not make assesment");
            }
        }

        public override void Execute()
        {
            base.Execute();

            var dialog = new SQLBeforeAndAfterViewer(_releasePotential.SqlCurrentConfiguration, _releasePotential.SqlExtracted, "Current Configuration", "Configuration when last run", "Sql Executed", MessageBoxButtons.OK);
            dialog.Show();
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.Diff);
        }
    }
}