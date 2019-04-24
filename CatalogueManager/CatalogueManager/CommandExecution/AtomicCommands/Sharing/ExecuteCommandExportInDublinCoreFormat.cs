// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.IO;
using CatalogueManager.ItemActivation;
using Rdmp.Core.CatalogueLibrary.Data;
using Rdmp.Core.CatalogueLibrary.DublinCore;
using ReusableLibraryCode.CommandExecution.AtomicCommands;

namespace CatalogueManager.CommandExecution.AtomicCommands.Sharing
{
    internal class ExecuteCommandExportInDublinCoreFormat : BasicUICommandExecution,IAtomicCommand
    {
        private readonly DublinCoreDefinition _definition;
        private FileInfo _toExport;
        readonly DublinCoreTranslater _translater = new DublinCoreTranslater();

        public ExecuteCommandExportInDublinCoreFormat(IActivateItems activator, Catalogue catalogue) : base(activator)
        {
            _definition = _translater.GenerateFrom(catalogue);
            UseTripleDotSuffix = true;
        }

        public override void Execute()
        {
            base.Execute();

            if ((_toExport = _toExport??SelectSaveFile("Dublin Core Xml|*.xml")) == null)
                return;

            using (var stream = File.OpenWrite(_toExport.FullName))
                _definition.WriteXml(stream);
        }
    }
}