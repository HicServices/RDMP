// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using SixLabors.ImageSharp;
using System.Linq;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Icons.IconProvision;
using ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands.CatalogueCreationCommands
{
    public class ExecuteCommandCreateNewCatalogueFromTableInfo : CatalogueCreationCommandExecution
    {
        private TableInfo _tableInfo;

        public ExecuteCommandCreateNewCatalogueFromTableInfo(IBasicActivateItems activator, TableInfo tableInfo) : base(activator)
        {
            _tableInfo = tableInfo;

            if (activator.CoreChildProvider.AllCatalogues.Any(c => c.Name.Equals(tableInfo.GetRuntimeName())))
                SetImpossible("There is already a Catalogue called '" + tableInfo.GetRuntimeName() + "'");
        }


        public override void Execute()
        {
            base.Execute();

            var cata = BasicActivator.CreateAndConfigureCatalogue(_tableInfo,null,"Existing Table",ProjectSpecific,TargetFolder);
            
            if (cata is DatabaseEntity de)
            {
                Publish(de);
                Emphasise(de);
            }
        }

        public override Image<Rgba32> GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.Catalogue, OverlayKind.Shortcut);
        }
    }
}