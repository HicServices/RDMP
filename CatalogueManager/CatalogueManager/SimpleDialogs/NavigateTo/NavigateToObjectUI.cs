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
using CatalogueLibrary.Providers;
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
        private readonly Dictionary<IMapsDirectlyToDatabaseTable, DescendancyList> _searchables;
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
            typeof(LoadStageNode),
            typeof(PreLoadDiscardedColumnsNode)
        };

        public NavigateToObjectUI(IActivateItems activator)
        {
            _activator = activator;
            _coreIconProvider = activator.CoreIconProvider;
            _favouriteProvider = _activator.FavouritesProvider;
            _magnifier = FamFamFamIcons.magnifier;
            InitializeComponent();

            _searchables = _activator.CoreChildProvider.GetAllSearchables();

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
                _matches = _searchables.Take(MaxMatches).Select(t => t.Key).ToList();
                return;
            }

            var tokens = tbFind.Text.Split(new char[]{' '},StringSplitOptions.RemoveEmptyEntries);


            List<int> integerTokens = new List<int>();

            foreach (string token in tokens)
            {
                int i;
                if(int.TryParse(token,out i))
                    integerTokens.Add(i);
            }
            
            var regexes = new List<Regex>();

            foreach (string token in tokens)
                regexes.Add(new Regex(Regex.Escape(token), RegexOptions.IgnoreCase));

            _matches = 
                _searchables.ToDictionary(
                    s=>s,
                    score=>ScoreMatches(score,integerTokens,regexes)
                    )
                .Where(score => score.Value > 0)
                .OrderByDescending(score => score.Value)
                .Take(MaxMatches)
                .Select(score => score.Key.Key)
                .ToList();

            Height = (int) ((_matches.Count*RowHeight) + DrawMatchesStartingAtY);

            Invalidate();
        }

        private static readonly int[] Weights = new int[] {64, 32, 16, 8, 4, 2, 1};

        private int ScoreMatches(KeyValuePair<IMapsDirectlyToDatabaseTable, DescendancyList> kvp, List<int> integerTokens, List<Regex> regexes)
        {
            int score = 0;

            //make a new list so we can destructively read it
            regexes = new List<Regex>(regexes);

            //match on ID of the head only
            foreach (int integerToken in integerTokens)
                if (kvp.Key.ID == integerToken)
                {
                    //matched on the ID (we could also match this in the tostring e.g. "project 132 my fishing project" where 132 is a number that is meaningful to the user only
                    var regex = regexes.SingleOrDefault(r => r.ToString().Equals(integerToken.ToString()));
                    if (regex != null)
                        regexes.Remove(regex);

                    score += Weights[0];
                }

            //match on the head vs the regex tokens
            if (IsMatchToString(regexes, kvp.Key))
                score += Weights[0];

            if (IsMatchType(regexes, kvp.Key))
                score += Weights[0];

            //match on the parents if theres a decendancy list
            if(kvp.Value != null)
            {
                var parents = kvp.Value.Parents;
                int numberOfParents = parents.Length;

                 //for each prime after the first apply it as a multiple of the parent match
                for (int i = 1 ; i< Weights.Length; i++)
                {
                    //if we have run out of parents
                    if (i > numberOfParents)
                        break;

                    var parent = parents[parents.Length - i];

                    if(parent != null)
                    {
                        if (IsMatchToString(regexes, parent))
                            score += Weights[i];

                        if (IsMatchType(regexes, parent))
                            score += Weights[i];
                    }
                }
            }

            //if there were unmatched regexes
            if (regexes.Any())
                return 0;

            return score;
        }

        private bool IsMatchType(List<Regex> regexes, object key)
        {
            return IsMatch(regexes, key.GetType().Name);
        }
        private bool IsMatchToString(List<Regex> regexes, object key)
        {
            return IsMatch(regexes, key.ToString());
        }
        private bool IsMatch(List<Regex> regexes, string str)
        {
            var match = regexes.FirstOrDefault(r => r.IsMatch(str));

            if (match != null)
            {
                regexes.Remove(match);
                return true;
            }
            return false;
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
