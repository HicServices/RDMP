using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.Nodes;
using CatalogueLibrary.Nodes.LoadMetadataNodes;
using CatalogueManager.AggregationUIs;
using CatalogueManager.Collections.Providers;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.ItemActivation.Emphasis;
using MapsDirectlyToDatabaseTable;
using ReusableUIComponents;
using ReusableUIComponents.Icons.IconProvision;

namespace CatalogueManager.SimpleDialogs.NavigateTo
{
    /// <summary>
    /// Allows you to search all objects in your database and rapidly select 1 which will be shown via the Emphasis system.
    /// </summary>
    public partial class NavigateToObjectUI : Form
    {
        private readonly IActivateItems _activator;
        private readonly Tuple<string, IMapsDirectlyToDatabaseTable>[] _searchables;
        private ICoreIconProvider _coreIconProvider;
        private FavouritesProvider _favouriteProvider;

        private const int MaxMatches = 30;
        private List<IMapsDirectlyToDatabaseTable> _matches;

        //drawing
        private int selectedIndex = 0;
        private const float DrawMatchesStartingAtY = 25;
        private const float RowHeight = 20;
        private Bitmap _magnifier;

        private static readonly Type[] TypesThatAreNotUsefulParents =
        {
            typeof(CatalogueItemsNode),
            typeof(DocumentationNode),
            typeof(AggregatesNode),
            typeof(CohortSetsNode),
            typeof(LoadMetadataScheduleNode),
            typeof(AllCataloguesUsedByLoadMetadataNode),
            typeof(AllProcessTasksUsedByLoadMetadataNode),
            typeof(LoadStageNode)

        };

        public NavigateToObjectUI(IActivateItems activator)
        {
            _activator = activator;
            _coreIconProvider = activator.CoreIconProvider;
            _favouriteProvider = _activator.FavouritesProvider;
            _magnifier = FamFamFamIcons.magnifier;
            InitializeComponent();

            _searchables = _activator.CoreChildProvider.GetAllSearchables().OfType<IMapsDirectlyToDatabaseTable>().Select(k => new Tuple<string, IMapsDirectlyToDatabaseTable>(k.ToString() + k.GetType().Name, k)).ToArray();

            tbFind.Focus();
            FetchMatches();
            StartPosition = FormStartPosition.CenterScreen;
            DoubleBuffered = true;
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if(e.Y<= DrawMatchesStartingAtY || e.Y > (RowHeight * MaxMatches )+ DrawMatchesStartingAtY)
                return;

            selectedIndex = RowIndexFromPoint(e.X, e.Y);
            EmphasiseAndClose();
        }

        protected override bool ProcessKeyPreview(ref Message m)
        {
            PreviewKey p = new PreviewKey(ref m, ModifierKeys);

            if (p.e != null)
            {

                if (p.e.KeyCode == Keys.Up)
                {
                    p.Trap(this);

                    if (p.IsKeyDownMessage)
                        MoveSelectionUp();
                }

                if (p.e.KeyCode == Keys.Down)
                {
                    p.Trap(this);

                    if (p.IsKeyDownMessage)
                        MoveSelectionDown();
                }

                if (p.e.KeyCode == Keys.Enter)
                {
                    EmphasiseAndClose();
                }

                if (p.e.KeyCode == Keys.Escape)
                {
                    Close();
                }
                
            }

            return base.ProcessKeyPreview(ref m);
        }

        private void EmphasiseAndClose()
        {
            if (selectedIndex >= _matches.Count)
                return;

            Close();
            _activator.RequestItemEmphasis(this, new EmphasiseRequest(_matches[selectedIndex]));
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            var before = selectedIndex;
            selectedIndex = RowIndexFromPoint(e.X, e.Y);
            
            if(before != selectedIndex)
                Invalidate();
        }

        private int RowIndexFromPoint(int x, int y)
        {
            y -= (int)DrawMatchesStartingAtY;

            return Math.Max(0,Math.Min(MaxMatches,(int) (y/RowHeight)));
        }

        private void MoveSelectionDown()
        {
            selectedIndex = Math.Min(_matches.Count-1, //don't go above the number matches returned
                Math.Min(MaxMatches - 1, //don't go above the max number of matches 
                selectedIndex + 1));
            Invalidate();
        }

        private void MoveSelectionUp()
        {
            selectedIndex = Math.Min(_matches.Count-1,  //if text has been typed then selectedIndex could be higher than the number of matches so set that as a roof
                Math.Max(0, //also don't go below 0
                    selectedIndex - 1)); 
            Invalidate();
        }


        protected override void OnDeactivate(EventArgs e)
        {
            this.Close();
        }

        private void tbFind_TextChanged(object sender, EventArgs e)
        {
            FetchMatches();
        }

        private void FetchMatches()
        {
            if (string.IsNullOrWhiteSpace(tbFind.Text))
            {
                _matches = _searchables.Take(MaxMatches).Select(t => t.Item2).ToList();
                return;
            }

            var tokens = tbFind.Text.Split(new char[]{' '},StringSplitOptions.RemoveEmptyEntries);
            var regexes = new List<Regex>();

            foreach (string token in tokens)
                regexes.Add(new Regex(Regex.Escape(token), RegexOptions.IgnoreCase));

            _matches = new List<IMapsDirectlyToDatabaseTable>();

            for (int i = 0; _matches.Count < MaxMatches && i < _searchables.Length; i++)
                if (regexes.All(r => r.IsMatch(_searchables[i].Item1)))
                    _matches.Add(_searchables[i].Item2);
            
            Height = (int) ((_matches.Count*RowHeight) + DrawMatchesStartingAtY);

            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            e.Graphics.DrawImage(_magnifier,0,0);

            float maxWidthUsedDuringRender = 0;

            if(_matches != null)
            {
                for (int i = 0; i < _matches.Count; i++)
                {
                    bool isFavourite = _favouriteProvider.IsFavourite(_matches[i]);
                    float currentRowStartY = DrawMatchesStartingAtY + (RowHeight*i);

                    var img = _coreIconProvider.GetImage(_matches[i],isFavourite?OverlayKind.FavouredItem:OverlayKind.None);
                    
                    e.Graphics.FillRectangle(i == selectedIndex ? new SolidBrush(SystemColors.Highlight) : Brushes.White, 1, currentRowStartY,Width, RowHeight);

                    string text = _matches[i].ToString();

                    //record how wide it is so we know how much space is left to draw parents
                    maxWidthUsedDuringRender = Math.Max(maxWidthUsedDuringRender,e.Graphics.MeasureString(text, Font).Width + 20);

                    e.Graphics.DrawImage(img,1,currentRowStartY);
                    e.Graphics.DrawString(text,Font,Brushes.Black,20,currentRowStartY );
                }

                //now draw parents
                for (int i = 0; i < _matches.Count; i++)
                {
                    //get first parent that isn't one of the explicitly useless parent types (I'd rather know the Catalogue of an AggregateGraph than to know it's an under an AggregatesGraphNode)                
                    var descendancy = _activator.CoreChildProvider.GetDescendancyListIfAnyFor(_matches[i]);


                    object lastParent = null;
                    if(descendancy != null)
                        lastParent = descendancy.Parents.LastOrDefault(parent => 
                            !TypesThatAreNotUsefulParents.Contains(parent.GetType())
                             &&
                            !(parent is CatalogueLibrary.Data.IContainer)
                           );

                    float currentRowStartY = DrawMatchesStartingAtY + (RowHeight*i);

                    if (lastParent != null)
                    {

                        ColorMatrix cm = new ColorMatrix();
                        cm.Matrix33 = 0.55f;
                        ImageAttributes ia = new ImageAttributes();
                        ia.SetColorMatrix(cm);

                        var rect = new Rectangle(Width - 20, (int)currentRowStartY, 19, 19);
                        var img = _coreIconProvider.GetImage(lastParent);

                        //draw the parents image on the right
                        e.Graphics.DrawImage(img, rect, 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, ia);

                        var horizontalSpaceAvailableToDrawTextInto = Width - (maxWidthUsedDuringRender + 20); 

                        string text = ShrinkTextToFitWidth(lastParent.ToString(),horizontalSpaceAvailableToDrawTextInto,e.Graphics);
                        var spaceRequiredForCurrentText = e.Graphics.MeasureString(text, Font).Width;

                        e.Graphics.DrawString(text,Font,Brushes.DarkGray,Width - (spaceRequiredForCurrentText+20) ,currentRowStartY);
                    }
                }
            }
        }

        private string ShrinkTextToFitWidth(string originalText, float horizontalSpaceAvailableToDrawTextInto, Graphics g)
        {

            //it fits without truncation
            if (g.MeasureString(originalText, Font).Width < horizontalSpaceAvailableToDrawTextInto)
                return originalText;

            var suffixWidth = g.MeasureString("...", Font).Width;
            
            while (g.MeasureString(originalText, Font).Width + suffixWidth > horizontalSpaceAvailableToDrawTextInto && originalText.Length > 1)
                //knock off a character
                originalText = originalText.Substring(0, originalText.Length - 1);

            return originalText + "...";
        }
    }
}
