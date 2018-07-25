using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.ImportExport;
using CatalogueLibrary.Data.Serialization;
using CatalogueLibrary.Repositories.Construction;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using MapsDirectlyToDatabaseTable.Attributes;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents;

namespace CatalogueManager.CommandExecution.AtomicCommands
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

        public Image GetImage(IIconProvider iconProvider)
        {
            return FamFamFamIcons.page_white_get;
        }
    }
}