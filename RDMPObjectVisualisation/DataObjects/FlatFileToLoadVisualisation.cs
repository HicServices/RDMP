using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using RDMPObjectVisualisation.Etier.IconHelper;
using ReusableLibraryCode;
using ReusableUIComponents;

namespace RDMPObjectVisualisation.DataObjects
{
    /// <summary>
    /// Input object for visualizing a file you are trying to load (any file type).  Used by PipelineDiagram and ConfigureAndExecutePipeline.
    /// 
    /// Double click the file to open it with your default application (e.g. xlsx would open in microsoft office).  Do not double click gigantic (1GB+) files as it is likely that your 
    /// default application (E.g. word/notepad) will struggle to open it.
    //
    /// </summary>
    public partial class FlatFileToLoadVisualisation : UserControl
    {
        private readonly FlatFileToLoad _value;

        public FlatFileToLoadVisualisation(FlatFileToLoad value)
        {
            _value = value;
            InitializeComponent();

            if (value == null)
                return;

            Icon icon = IconReader.GetFileIcon(value.File.FullName, IconReader.IconSize.Large, true);
            pictureBox1.Image = icon.ToBitmap();


            lblFilename.Text = value.File.Name;
            lblFileSize.Text = GetFileSizeSensible();

            this.Width = 10 + lblFilename.PreferredWidth;
            
        }

        private string GetFileSizeSensible()
        {
            return UsefulStuff.GetHumanReadableByteSize(_value.File.Length);
        }

        private void pictureBox1_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                Process.Start(_value.File.FullName);
            }
            catch (Exception exception)
            {
                ExceptionViewer.Show(exception);
            }
        }
    }

}
