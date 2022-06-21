// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using MapsDirectlyToDatabaseTable;
using Rdmp.Core.Curation.Data;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{
    /// <summary>
    /// Records as an ExtendedProperty that a given object is replaced by another.
    /// Typically used to forward users of Deprecated items to the new live version
    /// </summary>
    public class ExecuteCommandIsReplacedBy : BasicCommandExecution, IAtomicCommand
    {
        /// <summary>
        /// Key for ExtendedProperty that indicates an object is replaced by another
        /// </summary>
        public const string ReplacedBy = "ReplacedBy";

        public IMapsDirectlyToDatabaseTable Deprecated { get; }
        public IMapsDirectlyToDatabaseTable Replacement { get; }

        private Type _type;

        public ExecuteCommandIsReplacedBy(IBasicActivateItems activator, IMapsDirectlyToDatabaseTable deprecated, IMapsDirectlyToDatabaseTable replacement) 
            : base(activator)
        {
            
            Deprecated = deprecated;
            Replacement = replacement;

            _type = deprecated.GetType();

            if(deprecated is IMightBeDeprecated m && !m.IsDeprecated)
            {
                SetImpossible($"{deprecated} is not marked IsDeprecated so no replacement can be specified");
            }

            if(replacement != null && replacement.GetType() != _type)
            {
                SetImpossible($"'{replacement}' cannot replace '{deprecated}' because it is a different object Type");
            }
        }

        public override void Execute()
        {
            base.Execute();

            var rep = Replacement;

            if(rep == null)
            {
                if(!BasicActivator.SelectObject(new DialogArgs{
                    AllowSelectingNull = true
                },BasicActivator.CoreChildProvider.AllCatalogues,out rep))
                {
                    // user cancelled
                    return;
                }
            }

            var cataRepo = BasicActivator.RepositoryLocator.CatalogueRepository;
            foreach(var existing in 
                cataRepo.GetAllObjectsWhere<ExtendedProperty>("Name",ReplacedBy)
                .Where(r=>r.IsReferenceTo(Deprecated)))
                {
                    // delete any old references to who we are replaced by
                    existing.DeleteInDatabase();
                }

                // store the ID of the thing that replaces us
                new ExtendedProperty(cataRepo,Deprecated,ReplacedBy,rep.ID);
        }
    }
}