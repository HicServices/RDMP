using BrightIdeasSoftware;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Rdmp.UI.SimpleDialogs
{
    public partial class ViewParentTreeDialog : Form
    {
        private readonly List<object> _tree;
        private readonly IBasicActivateItems _activator;

        public ViewParentTreeDialog(IBasicActivateItems activator, List<object> tree)
        {
            _tree = tree;
            _activator = activator;
            InitializeComponent();
            tlv.CanExpandGetter = delegate (object x)
            {
                return tree.IndexOf(x) < tree.Count() - 1;
            };
            tlv.ChildrenGetter = delegate (object x)
            {
                var item = tree[tree.IndexOf(x) + 1];
                return new List<object>() { item };
            };
            tlv.Roots = new List<object>() { tree[0] };
            tlvParentColumn.FillsFreeSpace = true;
            tlv.ExpandAll();
        }

        private Bitmap ImageGetter(object rowObject)
        {
            var image = _activator.CoreIconProvider.GetImage(rowObject);
            return image.ImageToBitmap();
        }

        private static bool Is<T>(object o, out T match)
        {
            while (true)
                switch (o)
                {
                    case T o1:
                        match = o1;
                        return true;
                    case IMasqueradeAs m:
                        o = m.MasqueradingAs();
                        continue;
                    default:
                        match = default;
                        return false;
                }
        }

        private void ClickHandler(object sender, CellClickEventArgs e)
        {
            if (e.HitTest.HitTestLocation == HitTestLocation.ExpandButton) return;
            if (e.RowIndex > _tree.Count) return;
            var item = _tree[e.RowIndex];
            if (Is(item, out IMapsDirectlyToDatabaseTable mt))
            {
                _activator.Activate(mt);
            }
        }
    }
}
