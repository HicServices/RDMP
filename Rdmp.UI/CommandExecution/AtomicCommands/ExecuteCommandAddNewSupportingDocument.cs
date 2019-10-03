// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Repositories;
using Rdmp.Core.Repositories.Construction;
using Rdmp.UI.Copying.Commands;
using Rdmp.UI.Icons.IconProvision;
using Rdmp.UI.ItemActivation;
using ReusableLibraryCode.Icons.IconProvision;

namespace Rdmp.UI.CommandExecution.AtomicCommands
{
    public class ExecuteCommandAddNewSupportingDocument : BasicUICommandExecution,IAtomicCommand
    {
        private readonly FileCollectionCommand _fileCollectionCommand;
        private readonly Catalogue _targetCatalogue;

        [UseWithObjectConstructor]
        public ExecuteCommandAddNewSupportingDocument(IActivateItems activator, Catalogue catalogue) : base(activator)
        {
            _targetCatalogue = catalogue;
        }
        public ExecuteCommandAddNewSupportingDocument(IActivateItems activator, FileCollectionCommand fileCollectionCommand, Catalogue targetCatalogue): base(activator)
        {
            _fileCollectionCommand = fileCollectionCommand;
            _targetCatalogue = targetCatalogue;
            var allExisting = targetCatalogue.GetAllSupportingDocuments(FetchOptions.AllGlobalsAndAllLocals);

            foreach (var doc in allExisting)
            {
                FileInfo filename = doc.GetFileName();
                
                if(filename == null)
                    continue;

                var collisions = _fileCollectionCommand.Files.FirstOrDefault(f => f.FullName.Equals(filename.FullName,StringComparison.CurrentCultureIgnoreCase));
                
                if(collisions != null)
                    SetImpossible("File '" + collisions.Name +"' is already a SupportingDocument (ID=" + doc.ID + " - '"+doc.Name+"')");
            }
        }

        public override string GetCommandHelp()
        {
            return "Marks a file on disk as useful for understanding the dataset and (optionally) copies into project extractions";
        }

        public override void Execute()
        {
            base.Execute();

            FileInfo[] files = null;

            if (_fileCollectionCommand != null)
                files = _fileCollectionCommand.Files;
            else
            {
                OpenFileDialog fileDialog = new OpenFileDialog();
                fileDialog.Multiselect = true;
                if (fileDialog.ShowDialog() == DialogResult.OK)
                    files = fileDialog.FileNames.Select(f => new FileInfo(f)).Where(fi => fi.Exists).ToArray();
                else
                    return;
            }

            List<SupportingDocument> created = new List<SupportingDocument>();
            foreach (var f in files)
            {
                var doc = new SupportingDocument((ICatalogueRepository)_targetCatalogue.Repository, _targetCatalogue, f.Name);
                doc.URL = new Uri(f.FullName);
                doc.SaveToDatabase();
                created.Add(doc);
            }

            Publish(_targetCatalogue);

            Emphasise(created.Last());
            
            foreach (var doc in created)
                Activate(doc);
        }
        public override Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.SupportingDocument, OverlayKind.Add);
        }
    }
}