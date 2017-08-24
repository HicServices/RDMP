using System.Drawing;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueManager.Collections.Providers;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.Menus.MenuItems;

namespace CatalogueManager.ObjectVisualisation
{
    /// <summary>
    /// Allows you to visualise a collection of RDMP objects in a thin ribbon along the top of another control.  These objects should help provide context for what is being seen in the rest
    /// of the control.  In addition to RDMP objects (graphs, cohorts etc) you can add strings that provide further context.
    /// </summary>
    public partial class RDMPObjectsRibbonUI : UserControl
    {
        private ICoreIconProvider _coreIconProvider;

        public RDMPObjectsRibbonUI()
        {
            InitializeComponent();
        }

        public void SetIconProvider(ICoreIconProvider coreIconProvider)
        {
            _coreIconProvider = coreIconProvider;
        }

        /// <summary>
        /// Adds the passed object into the ribbon as the next item, if icon provider has been set and image is null then the icon provider will be asked for an icon.  If an image is 
        /// specified then it will always override the icon provider.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="image"></param>
        public void Add(DatabaseEntity entity, Image image = null)
        {
            if (image == null && _coreIconProvider != null)
                image = _coreIconProvider.GetImage(entity); //can return null

            //if there is still no image use a red X
            if (image == null)
                image = FamFamFamIcons.cancel;

            Add(entity.ToString(), image);
        }

        public void Add(string text)
        {
            Add(text, FamFamFamIcons.text_align_left);
        }

        private void Add(string text, Image image)
        {
            var p = new Panel();

            PictureBox pb = new PictureBox();
            pb.Image = image;
            pb.Width = 20;
            pb.Height = 20;

            var lbl = new Label();
            lbl.Text = text;
            lbl.Left = 20;
            lbl.Height = 20;
            lbl.Width = lbl.PreferredWidth;
            lbl.TextAlign = ContentAlignment.MiddleLeft;
            var textWidth = lbl.PreferredWidth;

            p.Height = 20;
            p.Width = textWidth + pb.Width;
            p.Controls.Add(pb);
            p.Controls.Add(lbl);
            
            flowLayoutPanel1.Controls.Add(p);
        }

        public void Clear()
        {
            flowLayoutPanel1.Controls.Clear();

        }
    }
}
