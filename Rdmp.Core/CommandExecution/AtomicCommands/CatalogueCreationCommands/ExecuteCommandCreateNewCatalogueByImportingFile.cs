// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Drawing;
using System.IO;
using FAnsi.Discovery;
using Rdmp.Core;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.Combining;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Repositories.Construction;
using ReusableLibraryCode.Icons.IconProvision;

namespace Rdmp.Core.CommandExecution.AtomicCommands.CatalogueCreationCommands
{
    public class ExecuteCommandCreateNewCatalogueByImportingFile : CatalogueCreationCommandExecution
    {
        private DiscoveredDatabase _targetDatabase;
        private IPipeline _pipeline;

        public CatalogueFolder TargetFolder { get; set; }

        public FileInfo File { get; private set; }

        private string _extractionIdentifier;

        /// <summary>
        /// Create a project specific Catalogue when command is executed by prompting the user to first pick a project
        /// </summary>
        public bool PromptForProject { get; set; }

        private void CheckFile()
        {
            if (File == null)
                return;

            if (!File.Exists)
                SetImpossible("File does not exist");
        }

        public ExecuteCommandCreateNewCatalogueByImportingFile(IBasicActivateItems activator, FileInfo file = null) : this(activator, file, null, null, null, null)
        {
        }

        [UseWithObjectConstructor]
        public ExecuteCommandCreateNewCatalogueByImportingFile(IBasicActivateItems activator,
            [DemandsInitialization("The file to load into the database")]
            FileInfo file,
            [DemandsInitialization("Name of a column in the file to be the IsExtractionIdentifier column or Null if it doesn't have one")]
            string extractionIdentifier,
            [DemandsInitialization("The database to upload the data into")]
            DiscoveredDatabase targetDatabase,
            [DemandsInitialization("Pipeline for reading the source file, applying any transforms and writting to the database")]
            Pipeline pipeline,
            [DemandsInitialization(Desc_ProjectSpecificParameter)]
            Project projectSpecific) : base(activator,projectSpecific)
        {
            File = file;
            _extractionIdentifier = extractionIdentifier;
            _targetDatabase = targetDatabase;
            _pipeline = pipeline;
            UseTripleDotSuffix = true;
            CheckFile();
        }


        public ExecuteCommandCreateNewCatalogueByImportingFile(IBasicActivateItems activator, FileCollectionCombineable file) : base(activator)
        {
            if (file.Files.Length != 1)
            {
                SetImpossible("Only one file can be imported at once");
                return;
            }

            File = file.Files[0];
            UseTripleDotSuffix = true;
            CheckFile();
        }


        public override void Execute()
        {
            base.Execute();

            if (PromptForProject)
                if (SelectOne(BasicActivator.RepositoryLocator.DataExportRepository, out Project p))
                    ProjectSpecific = p;
                else
                    return; //dialogue was cancelled



            var dialog = new CreateNewCatalogueByImportingFileUI(Activator, this)
            {
                TargetFolder = TargetFolder
            };

            if (_project != null)
                dialog.SetProjectSpecific(_project);

            dialog.ShowDialog();
        }

        public override Image GetImage(IIconProvider iconProvider)
        {
            return ProjectSpecific != null ?
                iconProvider.GetImage(RDMPConcept.ProjectCatalogue, OverlayKind.Add) :
                iconProvider.GetImage(RDMPConcept.Catalogue, OverlayKind.Add);
        }


        public override string GetCommandHelp()
        {
            return GlobalStrings.CreateNewCatalogueByImportingFileHelp;
        }

        public override string GetCommandName()
        {
            return OverrideCommandName ?? GlobalStrings.CreateNewCatalogueByImportingFile;
        }
    }
}