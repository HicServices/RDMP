// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using System.Windows.Forms;
using MapsDirectlyToDatabaseTable;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Providers;
using Rdmp.UI.SimpleDialogs;
using ReusableLibraryCode.Icons.IconProvision;

namespace Rdmp.UI.Menus
{
    [System.ComponentModel.DesignerCategory("")]
    class ExtractableDataSetPackageMenu : RDMPContextMenuStrip
    {
        private readonly ExtractableDataSetPackage _package;
        private readonly DataExportChildProvider _childProvider;

        public ExtractableDataSetPackageMenu(RDMPContextMenuStripArgs args, ExtractableDataSetPackage package): base(args, package)
        {
            _package = package;
            _childProvider = (DataExportChildProvider) _activator.CoreChildProvider;
            Items.Add("Add ExtractableDataSet(s) to Package", _activator.CoreIconProvider.GetImage(RDMPConcept.ExtractableDataSet, OverlayKind.Link), AddExtractableDatasetToPackage);

        }

        private void AddExtractableDatasetToPackage(object sender, EventArgs e)
        {
            var packageManager = _activator.RepositoryLocator.DataExportRepository.PackageManager;
            var notInPackage = _childProvider.ExtractableDataSets.Except(packageManager.GetAllDataSets(_package, _childProvider.ExtractableDataSets));

            var dialog = new SelectDialog<IMapsDirectlyToDatabaseTable>(_activator, notInPackage, false, false);
            dialog.AllowMultiSelect = true;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                foreach (ExtractableDataSet ds in dialog.MultiSelected.Cast<ExtractableDataSet>())
                    packageManager.AddDataSetToPackage(_package, ds);

                //package contents changed
                if(dialog.MultiSelected.Any())
                    Publish(_package);
            }
        }
    }
}