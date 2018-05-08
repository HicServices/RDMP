using System.Windows.Forms;
using CatalogueLibrary.Data;

namespace RDMPObjectVisualisation.DataObjects
{
    /// <summary>
    /// Input object for visualizing a file you are trying to load (any file type).  Used by PipelineDiagram and ConfigureAndExecutePipeline.
    /// 
    /// <para>Double click the file to open it with your default application (e.g. xlsx would open in microsoft office).  Do not double click gigantic (1GB+) files as it is likely that your 
    /// default application (E.g. word/notepad) will struggle to open it.</para>
    ///
    /// </summary>
    public partial class ExtractionInformationVisualisation : UserControl
    {
        public ExtractionInformationVisualisation(ExtractionInformation value)
        {
            InitializeComponent();

            lblName.Text = value.CatalogueItem.Catalogue + "." + value.GetRuntimeName();
            Width = lblName.PreferredWidth + 6;
        }
    }

}
