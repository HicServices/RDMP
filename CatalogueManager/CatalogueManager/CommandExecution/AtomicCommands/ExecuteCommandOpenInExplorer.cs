using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.Data;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents.CommandExecution.AtomicCommands;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandOpenInExplorer:BasicUICommandExecution,IAtomicCommand
    {
        private readonly FileInfo _file;
        private readonly DirectoryInfo _dir;
        
        public ExecuteCommandOpenInExplorer(IActivateItems activator,DirectoryInfo dir) : base(activator)
        {
            _dir = dir; 

            if (_dir == null || !_dir.Exists)
                SetImpossible("Directory not found");
        }
        public ExecuteCommandOpenInExplorer(IActivateItems activator, FileInfo file): base(activator)
        {
            _file = file;

            if(_file == null || !_file.Exists)
                SetImpossible("File not found");
        }

        public override void Execute()
        {
            base.Execute();

            if(_file != null)
                UsefulStuff.GetInstance().ShowFileInWindowsExplorer(_file);

            if(_dir != null)
                UsefulStuff.GetInstance().ShowFolderInWindowsExplorer(_dir);
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.CatalogueFolder);
        }
    }
}
