using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.SimpleDialogs.NavigateTo;
using CohortManager.CommandExecution.AtomicCommands;
using DataExportManager.CommandExecution.AtomicCommands;
using NUnit.Framework;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableUIComponents.CommandExecution.AtomicCommands;
using Rhino.Mocks.Constraints;
using Tests.Common;

namespace CatalogueLibraryTests.SourceCodeEvaluation
{
    public class RunUITests:DatabaseTests
    {
        private Type[] allowedToBeIncompatible
            = new[]
            {
                typeof(ExecuteCommandExportObjectsToFileUI),
                typeof(ExecuteCommandRunChecksInPopupWindow),
                typeof(ExecuteCommandShow),
                typeof(ExecuteCommandSetDataAccessContextForCredentials),
                typeof(ExecuteCommandActivate),
                typeof(ExecuteCommandCreateNewExternalDatabaseServer),
                typeof(ExecuteCommandCreateNewPipeline),
                typeof(ExecuteCommandDelete),
                typeof(ExecuteCommandEditPipeline),
                typeof(ExecuteCommandRename),
                typeof(ExecuteCommandSetPipeline),
                typeof(ExecuteCommandShowKeywordHelp),
                typeof(ExecuteCommandCollapseChildNodes),
                typeof(ExecuteCommandExpandAllNodes),
                typeof(ExecuteCommandViewDependencies),
                typeof(ExecuteCommandViewCohortAggregateGraph),
                typeof(ExecuteCommandExecuteExtractionAggregateGraph)
            };

        [Test]
        public void AllCommandsCompatible()
        {
            List<Exception> ex;
            IEnumerable<Type> commands = RepositoryLocator.CatalogueRepository.MEF.GetAllTypesFromAllKnownAssemblies(out ex).Where(t => typeof(IAtomicCommand).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface);

            var notSupported = commands.Where(c => !RunUI.IsSupported(c.GetConstructors()[0])).Except(allowedToBeIncompatible).ToList();
            
            Assert.AreEqual(0,notSupported.Count,"The following commands were not compatible with RunUI:" + Environment.NewLine + string.Join(Environment.NewLine,notSupported.Select(t=>t.Name)));

        }
    }
}
