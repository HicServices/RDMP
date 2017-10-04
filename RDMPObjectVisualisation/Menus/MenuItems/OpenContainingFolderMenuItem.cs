using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace RDMPObjectVisualisation.Menus.MenuItems
{
    [System.ComponentModel.DesignerCategory("")]
    public class OpenContainingFolderMenuItem:ToolStripMenuItem
    {
        private readonly FileInfo _file;

        public OpenContainingFolderMenuItem(FileInfo file):base("Open Containing Folder")
        {
            _file = file;
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);

            string argument = "/select, \"" + _file.FullName + "\"";

            Process.Start("explorer.exe", argument);
        }
    }
}
