using System.Drawing;
using CatalogueLibrary.Data;
using CatalogueManager.ExtractionUIs.FilterUIs.ParameterUIs;
using CatalogueManager.ExtractionUIs.FilterUIs.ParameterUIs.Options;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandViewSqlParameters:BasicUICommandExecution,IAtomicCommand
    {
        private readonly ICollectSqlParameters _collector;

        public ExecuteCommandViewSqlParameters(IActivateItems activator,ICollectSqlParameters collector):base(activator)
        {
            _collector = collector;
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.ParametersNode);
        }

        public override void Execute()
        {
            var parameterCollectionUI = new ParameterCollectionUI();

            ParameterCollectionUIOptionsFactory factory = new ParameterCollectionUIOptionsFactory();
            var options = factory.Create(_collector);
            parameterCollectionUI.SetUp(options);

            Activator.ShowWindow(parameterCollectionUI, true);
        }
    }
}