using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.Data.DataLoad;
using CatalogueManager.Icons.IconProvision;
using ReusableUIComponents;

namespace CatalogueManager.DataLoadUIs.LoadMetadataUIs
{
    /// <summary>
    /// Icon for describing something relates to a given data LoadStage (e.g. AdjustRAW, AdjustSTAGING etc)
    /// </summary>
    [TechnicalUI]
    public partial class LoadStageIconUI : UserControl
    {
        public LoadStageIconUI()
        {
            InitializeComponent();
        }

        public void Setup(ICoreIconProvider iconProvider,LoadStage stage)
        {
            pictureBox1.Image = iconProvider.GetImage(stage);
            lblLoadStage.Text = stage.ToString();
            this.Width = lblLoadStage.Right;
        }
    }
}
