// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using MapsDirectlyToDatabaseTable;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rdmp.Core.CommandExecution
{
    /// <summary>
    /// Builds lists of <see cref="IAtomicCommand"/> for any given RDMP object
    /// </summary>
    public class AtomicCommandFactory
    {
        IBasicActivateItems _activator;

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
            if(o is Catalogue c)
            {
                yield return new ExecuteCommandAddNewSupportingSqlTable(_activator, c);
                yield return new ExecuteCommandAddNewSupportingDocument(_activator, c);
                yield return new ExecuteCommandAddNewAggregateGraph(_activator, c);
                yield return new ExecuteCommandAddNewCatalogueItem(_activator, c);
                                        
                yield return new ExecuteCommandChangeExtractability(_activator, c);
                yield return new ExecuteCommandMakeCatalogueProjectSpecific(_activator,c,null);
                yield return new ExecuteCommandMakeProjectSpecificCatalogueNormalAgain(_activator, c);
                yield return new ExecuteCommandSetExtractionIdentifier(_activator,c);
                                        
                yield return new ExecuteCommandExportObjectsToFile(_activator, new[] {c});
                yield return new ExecuteCommandExtractMetadata(_activator, new []{ c},null,null,null,false,null);
            }

			if(o is IDeleteable d)
				yield return new ExecuteCommandDelete(_activator,d);
        }
    }
}
