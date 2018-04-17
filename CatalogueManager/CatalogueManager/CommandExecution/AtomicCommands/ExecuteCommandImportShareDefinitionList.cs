using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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
                    {
                        var text = File.ReadAllText(f);
                        var toImport = (List<ShareDefinition>)JsonConvertExtensions.DeserializeObject(text, typeof(List<ShareDefinition>), Activator.RepositoryLocator);
                        
                        foreach (ShareDefinition sd in toImport)
                        {
                            try
                            {
                                var objectConstructor = new ObjectConstructor();
                                objectConstructor.ConstructIfPossible(sd.Type, shareManager, sd);
                            }
                            catch (Exception e)
                            {
                                throw new Exception("Error constructing " + sd.Type ,e);
                            }
                        }
                        
                    }
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