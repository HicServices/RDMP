using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data.ImportExport;
using CatalogueLibrary.Data.Serialization;
using CatalogueLibrary.Repositories.Construction;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
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

        public Image GetImage(IIconProvider iconProvider)
        {
            return FamFamFamIcons.page_white_get;
        }
    }
}