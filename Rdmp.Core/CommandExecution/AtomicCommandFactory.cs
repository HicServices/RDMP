// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using MapsDirectlyToDatabaseTable;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandExecution.AtomicCommands.CatalogueCreationCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rdmp.Core.CommandExecution
{
    /// <summary>
    /// Builds lists of <see cref="IAtomicCommand"/> for any given RDMP object
    /// </summary>
    public class AtomicCommandFactory
    {
        IBasicActivateItems _activator;

        public const string Add = "Add";
        public const string Extraction = "Extractability";
        public const string Metadata = "Metadata";
        public AtomicCommandFactory(IBasicActivateItems activator)
        {
            _activator = activator;
        }

        /// <summary>
        /// Returns all commands that could be run involving <paramref name="o"/> in order of most useful to least useful
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public IEnumerable<IAtomicCommand> CreateCommands(object o)
        {
            return GetCommandsWithPresentation(o).Select(p=>p.Command);
        }

        public IEnumerable<CommandPresentation> GetCommandsWithPresentation(object o)
        {
            if(o is Catalogue c)
            {
                yield return new CommandPresentation(new ExecuteCommandAddNewSupportingSqlTable(_activator, c),Add);
                yield return new CommandPresentation(new ExecuteCommandAddNewSupportingDocument(_activator, c),Add);
                yield return new CommandPresentation(new ExecuteCommandAddNewAggregateGraph(_activator, c),Add);
                yield return new CommandPresentation(new ExecuteCommandAddNewCatalogueItem(_activator, c),Add);
                                        
                yield return new CommandPresentation(new ExecuteCommandChangeExtractability(_activator, c),Extraction);
                yield return new CommandPresentation(new ExecuteCommandMakeCatalogueProjectSpecific(_activator,c,null),Extraction);
                yield return new CommandPresentation(new ExecuteCommandMakeProjectSpecificCatalogueNormalAgain(_activator, c),Extraction);
                yield return new CommandPresentation(new ExecuteCommandSetExtractionIdentifier(_activator,c),Extraction);
                                        
                yield return new CommandPresentation(new ExecuteCommandExportObjectsToFile(_activator, new[] {c}),Metadata);
                yield return new CommandPresentation(new ExecuteCommandExtractMetadata(_activator, new []{ c},null,null,null,false,null),Metadata);
            }

            if(o is AggregateConfiguration ac)
            {
                yield return new CommandPresentation(new ExecuteCommandDisableOrEnable(_activator, ac));
                yield return new CommandPresentation(new ExecuteCommandAddNewFilterContainer(_activator,ac));
                yield return new CommandPresentation(new ExecuteCommandImportFilterContainerTree(_activator,ac));
                yield return new CommandPresentation(new ExecuteCommandCreateNewFilter(_activator,ac));
                yield return new CommandPresentation(new ExecuteCommandCreateNewFilterFromCatalogue(_activator,ac));
                
                yield return new CommandPresentation(new ExecuteCommandSetFilterTreeShortcut(_activator,ac));
                yield return new CommandPresentation(new ExecuteCommandSetFilterTreeShortcut(_activator,ac,null){OverrideCommandName="Clear Filter Tree Shortcut" });
            
                yield return new CommandPresentation(new ExecuteCommandCreateNewCatalogueByExecutingAnAggregateConfiguration(_activator,ac));
            }

			if(o is IDeleteable d)
				yield return new CommandPresentation(new ExecuteCommandDelete(_activator,d));
        }
    }
}
