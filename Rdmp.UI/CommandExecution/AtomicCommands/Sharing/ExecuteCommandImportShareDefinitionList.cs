// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using MapsDirectlyToDatabaseTable.Attributes;
using Rdmp.Core.CatalogueLibrary.Data;
using Rdmp.Core.CatalogueLibrary.Data.ImportExport;
using Rdmp.Core.CatalogueLibrary.Data.Serialization;
using Rdmp.UI.Icons.IconProvision;
using Rdmp.UI.ItemActivation;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents.Dialogs;

namespace Rdmp.UI.CommandExecution.AtomicCommands.Sharing
{
    public class ExecuteCommandImportShareDefinitionList : BasicUICommandExecution,IAtomicCommand
    {
        public ExecuteCommandImportShareDefinitionList(IActivateItems activator):base(activator)
        {
            
        }

        public override void Execute()
        {
            base.Execute();

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Sharing Definition File (*.sd)|*.sd";
            ofd.Multiselect = true;


            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    ShareManager shareManager = new ShareManager(Activator.RepositoryLocator);
                    shareManager.LocalReferenceGetter = LocalReferenceGetter;
                    foreach (var f in ofd.FileNames)
                        using (var stream = File.Open(f, FileMode.Open))
                            shareManager.ImportSharedObject(stream);
                }
                catch (Exception e)
                {
                    ExceptionViewer.Show("Error importing file(s)",e);
                }
            }
        }

        public override string GetCommandHelp()
        {
            return "Import serialized RDMP objects that have been shared with you in a share definition file.  If you already have the objects then they will be updated to match the file.";
        }

        private int? LocalReferenceGetter(PropertyInfo property, RelationshipAttribute relationshipAttribute,ShareDefinition shareDefinition)
        {
            MessageBox.Show("Choose a local object for '" + property + "' on " + Environment.NewLine
                            +
                            string.Join(Environment.NewLine,
                                shareDefinition.Properties.Select(kvp => kvp.Key + ": " + kvp.Value)));

            var requiredType = relationshipAttribute.Cref;

            if (Activator.RepositoryLocator.CatalogueRepository.SupportsObjectType(requiredType))
            {
                var selected = SelectOne(Activator.RepositoryLocator.CatalogueRepository.GetAllObjects(requiredType).Cast<DatabaseEntity>().ToArray());
                if (selected != null)
                    return selected.ID;
            }

            if (Activator.RepositoryLocator.DataExportRepository.SupportsObjectType(requiredType))
            {
                var selected = SelectOne(Activator.RepositoryLocator.DataExportRepository.GetAllObjects(requiredType).Cast<DatabaseEntity>().ToArray());
                if (selected != null)
                    return selected.ID;
            }

            return null;
        }

        public override Image GetImage(IIconProvider iconProvider)
        {
            return FamFamFamIcons.page_white_get;
        }
    }
}