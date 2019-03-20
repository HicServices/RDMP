// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueManager.Copying.Commands;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandAddNewCatalogueItem : BasicUICommandExecution,IAtomicCommand
    {
        private Catalogue _catalogue;
        private ColumnInfo[] _columnInfos;
        private HashSet<int> _existingColumnInfos;

        public ExecuteCommandAddNewCatalogueItem(IActivateItems activator, Catalogue catalogue,ColumnInfoCommand colInfo) : this(activator,catalogue,colInfo.ColumnInfos)
        {
            
        }

        public ExecuteCommandAddNewCatalogueItem(IActivateItems activator, Catalogue catalogue,params ColumnInfo[] columnInfos) : base(activator)
        {
            _catalogue = catalogue;

            _existingColumnInfos = new HashSet<int>(_catalogue.CatalogueItems.Select(ci=>ci.ColumnInfo_ID).Where(col=>col.HasValue).Select(v=>v.Value).Distinct().ToArray());

            _columnInfos = columnInfos;

            if(_columnInfos.Length > 0 && _columnInfos.All(AlreadyInCatalogue))
                SetImpossible("ColumnInfo(s) are already in Catalogue");
        }


        public override string GetCommandHelp()
        {
            return "Creates a new virtual column in the dataset, this is the first stage to making a new column extractable or defining a new extraction transform";
        }

        public override void Execute()
        {
            base.Execute();
        
            //if we have not got an explicit one to import let the user pick one
            if (_columnInfos.Length == 0)
            {
                MessageBox.Show("Select which column the new CatalogueItem will describe/extract", "Choose underlying Column");

                ColumnInfo columnInfo;
                string text;

                if(SelectOne(Activator.CoreChildProvider.AllColumnInfos,out columnInfo))
                    if(TypeText("Name", "Type a name for the new CatalogueItem", 500, columnInfo.GetRuntimeName(),out text))
                    {
                        var ci = new CatalogueItem(Activator.RepositoryLocator.CatalogueRepository, _catalogue, "New CatalogueItem " + Guid.NewGuid());
                        ci.Name = text;
                        ci.SetColumnInfo(columnInfo);
                        ci.SaveToDatabase();

                        Publish(_catalogue);
                        Emphasise(ci,int.MaxValue);
                    }
            }
            else
            {
                foreach (ColumnInfo columnInfo in _columnInfos)
                {
                    if(AlreadyInCatalogue(columnInfo))
                        continue;

                    var ci = new CatalogueItem(Activator.RepositoryLocator.CatalogueRepository, _catalogue, columnInfo.Name);
                    ci.SetColumnInfo(columnInfo);
                    ci.SaveToDatabase();
                }

                Publish(_catalogue);
            }
        }

        private bool AlreadyInCatalogue(ColumnInfo candidate)
        {
            return _existingColumnInfos.Contains(candidate.ID);
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.CatalogueItem, OverlayKind.Add);
        }
    }
}