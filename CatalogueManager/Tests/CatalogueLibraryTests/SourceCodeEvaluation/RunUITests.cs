using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.CommandExecution.AtomicCommands.PluginCommands;
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
        private List<Type> allowedToBeIncompatible
            = new List<Type>(new[]
            {
                typeof(ExecuteCommandExportObjectsToFileUI),
                typeof(ExecuteCommandRunChecksInPopupWindow),
                typeof(ExecuteCommandShow),
                typeof(ExecuteCommandSetDataAccessContextForCredentials),
                typeof(ExecuteCommandActivate),
                typeof(ExecuteCommandCreateNewExternalDatabaseServer),
                typeof(ExecuteCommandDelete),
                typeof(ExecuteCommandRename),
                typeof(ExecuteCommandSetPipeline),
                typeof(ExecuteCommandShowKeywordHelp),
                typeof(ExecuteCommandCollapseChildNodes),
                typeof(ExecuteCommandExpandAllNodes),
                typeof(ExecuteCommandViewDependencies),
                typeof(ExecuteCommandViewCohortAggregateGraph),
                typeof(ExecuteCommandExecuteExtractionAggregateGraph)
            });

        [Test]
        public void AllCommandsCompatible()
        {
            List<Exception> ex;
            
            Console.WriteLine("Looking in"+ typeof (ExecuteCommandCreateNewExtractableDataSetPackage).Assembly);
            Console.WriteLine("Looking in" + typeof(ExecuteCommandViewCohortAggregateGraph).Assembly);
            Console.WriteLine("Looking in" + typeof(ExecuteCommandUnpin).Assembly);
            Console.WriteLine("Looking in" + typeof(PluginAtomicCommand).Assembly);

            allowedToBeIncompatible.AddRange(RunUI.GetIgnoredCommands());

            var notSupported = RepositoryLocator.CatalogueRepository.MEF.GetAllTypesFromAllKnownAssemblies(out ex)
                .Where(t=>typeof(IAtomicCommand).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface) //must be something we would normally expect to be a supported Type
                .Where(t => !RunUI.IsSupported(t)) //but for some reason isn't
                .Except(allowedToBeIncompatible) //and isn't a permissable one
                .ToArray();
            
            Assert.AreEqual(0,notSupported.Length,"The following commands were not compatible with RunUI:" + Environment.NewLine + string.Join(Environment.NewLine,notSupported.Select(t=>t.Name)));

            var supported = RepositoryLocator.CatalogueRepository.MEF.GetAllTypesFromAllKnownAssemblies(out ex).Where(RunUI.IsSupported).ToArray();

            Console.WriteLine("The following commands are supported:" + Environment.NewLine + string.Join(Environment.NewLine,supported.Select(cmd=>cmd.Name)));

        }
    }
}
