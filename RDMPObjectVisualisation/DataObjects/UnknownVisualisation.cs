using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using RDMPObjectVisualisation.Etier.IconHelper;
using RDMPObjectVisualisation.Menus.MenuItems;
using ReusableLibraryCode;
using ReusableUIComponents;

namespace RDMPObjectVisualisation.DataObjects
{
    /// <summary>
    /// Input object for visualizing a file you are trying to load (any file type).  Used by PipelineDiagram and ConfigureAndExecutePipeline.
    /// 
    /// Double click the file to open it with your default application (e.g. xlsx would open in microsoft office).  Do not double click gigantic (1GB+) files as it is likely that your 
    /// default application (E.g. word/notepad) will struggle to open it.
    ///
    /// </summary>
    public partial class UnknownObjectVisualisation : UserControl
    {
        public UnknownObjectVisualisation(object value)
        {
            InitializeComponent();

            lblType.Text = value.GetType().Name;
            lblToString.Text = value.ToString();
        }
    }

}
