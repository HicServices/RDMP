using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AutocompleteMenuNS;
using CatalogueLibrary.Nodes;
using CatalogueLibrary.Nodes.LoadMetadataNodes;
using CatalogueLibrary.Providers;
using CatalogueManager.AggregationUIs;
using CatalogueManager.AutoComplete;
using CatalogueManager.Collections.Providers;
using CatalogueManager.Collections.Providers.Filtering;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.ItemActivation.Emphasis;
using DataExportLibrary.Data;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents;
using ReusableUIComponents.ScintillaHelper;
using ScintillaNET;
using IContainer = CatalogueLibrary.Data.IContainer;

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

        private Scintilla _scintilla;

        private const int MaxMatches = 30;
        private List<IMapsDirectlyToDatabaseTable> _matches;

        //drawing
        private int keyboardSelectedIndex = 0;
        private int mouseSelectedIndex = 0;

        Color keyboardSelectionColor = Color.FromArgb(210,230,255);
        Color mouseSelectionColor = Color.FromArgb(230, 245, 251);

        private const float DrawMatchesStartingAtY = 25;
        private const float RowHeight = 20;
        
        const int DiagramTabDistance = 20;

        private Bitmap _magnifier;
        private int _diagramBottom;


        Task _lastFetchTask = null;
        private CancellationTokenSource _lastCancellationToken;
        private AutoCompleteProvider _autoCompleteProvider;
        private Type[] _types;
        private string[] _typeNames;


        private static HashSet<Type> TypesThatAreNotUsefulParents = new HashSet<Type>(
            new []
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
        });

        private bool _isClosed;
        private bool _skipEnter;
        private bool _skipEscape;

        public static void RecordThatTypeIsNotAUsefulParentToShow(Type t)
        {
            if(!TypesThatAreNotUsefulParents.Contains(t))
                TypesThatAreNotUsefulParents.Add(t);
        }
        public NavigateToObjectUI(IActivateItems activator, string initialSearchQuery = null)
        {
            _activator = activator;
            _coreIconProvider = activator.CoreIconProvider;
            _favouriteProvider = _activator.FavouritesProvider;
            _magnifier = FamFamFamIcons.magnifier;
            InitializeComponent();

            _searchables = _activator.CoreChildProvider.GetAllSearchables();
            
            ScintillaTextEditorFactory factory = new ScintillaTextEditorFactory();
            _scintilla = factory.Create();
            panel1.Controls.Add(_scintilla);
            
            _scintilla.Focus();
            _scintilla.Text = initialSearchQuery;
            
            _scintilla.TextChanged += tbFind_TextChanged;
            _scintilla.PreviewKeyDown += _scintilla_PreviewKeyDown;
            _scintilla.KeyUp += _scintilla_KeyUp;

            _scintilla.Margins[0].Width = 0;//dont show line number

            _scintilla.ClearCmdKey(Keys.Enter);
            _scintilla.ClearCmdKey(Keys.Up);
            _scintilla.ClearCmdKey(Keys.Down);

            FetchMatches(initialSearchQuery,CancellationToken.None);
            StartPosition = FormStartPosition.CenterScreen;
            DoubleBuffered = true;
            
            _types = _searchables.Keys.Select(k => k.GetType()).Distinct().ToArray();
            _typeNames = _types.Select(t => t.Name).ToArray();

            _autoCompleteProvider = new AutoCompleteProvider(_activator);
            foreach (Type t in _types)
                _autoCompleteProvider.Add(t);

            _autoCompleteProvider.RegisterForEvents(_scintilla);
        }

        void _scintilla_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            var autoCompleteShowing = _autoCompleteProvider.IsShowing();
            
            _skipEnter = e.KeyCode == Keys.Enter && autoCompleteShowing;
            _skipEscape = e.KeyCode == Keys.Escape && autoCompleteShowing;
        }
        
        void ApplySyntaxHighlighting()
        {
            if(_isClosed)
                return;
            
            var startPos = 0;
            var endPos = _scintilla.TextLength;

            _scintilla.Styles[1].ForeColor = Color.Blue;

            _scintilla.StartStyling(startPos);
            var text = _scintilla.GetTextRange(startPos, endPos);

            int charPos = 0;
            foreach (string s in text.Split(' '))
            {
                if (_typeNames.Contains(s))
                    _scintilla.SetStyling(s.Length, 1);
                else
                    _scintilla.SetStyling(s.Length, 0);

                charPos += s.Length + 1; //for the space

                //deal with no trailing whitespace
                if(charPos + startPos <= endPos)
                    _scintilla.SetStyling(1, 0); //for the space
            }
        }

        void _scintilla_KeyUp(object sender, KeyEventArgs e)
        {
            if (_autoCompleteProvider.IsShowing())
                return;

            ApplySyntaxHighlighting();
            
            if (e.KeyCode == Keys.Up)
            {
                e.Handled = true;
                MoveSelectionUp();
            }

            if (e.KeyCode == Keys.Down)
            {
                e.Handled = true;
                MoveSelectionDown();
            }

            if (e.KeyCode == Keys.Enter && !_skipEnter)
            {
                e.Handled = true;
                EmphasiseAndClose(keyboardSelectedIndex);
            }
            
            if (e.KeyCode == Keys.Escape)
            {
                if(_skipEscape)
                    return;

                e.Handled = true;
                Close();
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
           base.OnMouseClick(e);

            if(e.Y<= DrawMatchesStartingAtY || e.Y > (RowHeight * MaxMatches )+ DrawMatchesStartingAtY)
                return;

            EmphasiseAndClose(RowIndexFromPoint(e.X, e.Y));
        }
        
        private void EmphasiseAndClose(int indexToSelect)
        {
            if (indexToSelect >= _matches.Count)
                return;

            Close();
            _activator.RequestItemEmphasis(this, new EmphasiseRequest(_matches[indexToSelect], int.MaxValue) { Pin = true });
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            var before = mouseSelectedIndex;
            mouseSelectedIndex = RowIndexFromPoint(e.X, e.Y);
            
            if(before != mouseSelectedIndex)
            {
                AdjustHeight();
                Invalidate();
            }
        }

        private void AdjustHeight()
        {
            SetClientSizeCore(ClientSize.Width, 
                    Math.Max(_diagramBottom,
                    (int)((_matches.Count * RowHeight) + DrawMatchesStartingAtY)));
        }

        private int RowIndexFromPoint(int x, int y)
        {
            y -= (int)DrawMatchesStartingAtY;

            return Math.Max(0,Math.Min(MaxMatches,(int) (y/RowHeight)));
        }

        private void MoveSelectionDown()
        {
            keyboardSelectedIndex = Math.Min(_matches.Count-1, //don't go above the number matches returned
                Math.Min(MaxMatches - 1, //don't go above the max number of matches 
                keyboardSelectedIndex + 1));
            Invalidate();
        }

        private void MoveSelectionUp()
        {
            keyboardSelectedIndex = Math.Min(_matches.Count-1,  //if text has been typed then selectedIndex could be higher than the number of matches so set that as a roof
                Math.Max(0, //also don't go below 0
                    keyboardSelectedIndex - 1)); 
            Invalidate();
        }


        protected override void OnDeactivate(EventArgs e)
        {
            if(_autoCompleteProvider.IsShowing())
                return;

            this.Close();
        }


        private void tbFind_TextChanged(object sender, EventArgs e)
        {
            //cancel the last execution if it has not completed yet
            if (_lastFetchTask != null && !_lastFetchTask.IsCompleted)
                _lastCancellationToken.Cancel();

            _lastCancellationToken = new CancellationTokenSource();

            var toFind = _scintilla.Text;

            _lastFetchTask = Task.Run(() => FetchMatches(toFind, _lastCancellationToken.Token))
                .ContinueWith(
                    (s) =>
                    {
                        if (_isClosed)
                            return;
                        
                        Invoke(new MethodInvoker(() =>
                        {
                            try
                            {
                                if(_isClosed)
                                    return;

                                AdjustHeight();

                                if (_isClosed)
                                    return;

                                Invalidate();
                            }
                            catch (ObjectDisposedException)
                            {
                                
                            }
                        }));
                        
                    });
        }

        private void FetchMatches(string text, CancellationToken cancellationToken)
        {

            if (string.IsNullOrWhiteSpace(text))
            {
                _matches = _searchables.Take(MaxMatches).Select(t => t.Key).ToList();
                return;
            }

            var scorer = new SearchablesMatchScorer();
            scorer.TypeNames = _typeNames;
            var scores = scorer.ScoreMatches(_searchables, text, cancellationToken);

            if (scores == null)
                return;

            _matches =
               scores
                .Where(score => score.Value > 0)
                .OrderByDescending(score => score.Value)
                .Take(MaxMatches)
                .Select(score => score.Key.Key)
                .ToList();
        }

        

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            float maxWidthUsedDuringRender = 0;
            int renderWidth = panel1.Right;

            //the descendancy diagram
            int diagramStartX = renderWidth + 10;
            int diagramStartY = panel1.Bottom;
            int diagramWidth = Right - diagramStartX;

            //draw Form background
            e.Graphics.FillRectangle(new SolidBrush(SystemColors.Control), 0, 0, renderWidth, (int)((_matches.Count * RowHeight) + DrawMatchesStartingAtY));

            //draw the search icon
            e.Graphics.DrawImage(_magnifier,0,0);
            
            //the match diagram
            if(_matches != null)
            {
                //draw the icon that represents the object being displayed and it's name on the right
                for (int i = 0; i < _matches.Count; i++)
                {
                    bool isFavourite = _favouriteProvider.IsFavourite(_matches[i]);
                    float currentRowStartY = DrawMatchesStartingAtY + (RowHeight*i);

                    var img = _coreIconProvider.GetImage(_matches[i],isFavourite?OverlayKind.FavouredItem:OverlayKind.None);

                    SolidBrush fillColor;

                    if (i == keyboardSelectedIndex)
                        fillColor = new SolidBrush(keyboardSelectionColor);
                    else if( i== mouseSelectedIndex)
                        fillColor = new SolidBrush(mouseSelectionColor);
                    else
                        fillColor = new SolidBrush(Color.White);

                    e.Graphics.FillRectangle(fillColor, 1, currentRowStartY, renderWidth, RowHeight);

                    string text = _matches[i].ToString();

                    //record how wide it is so we know how much space is left to draw parents
                    maxWidthUsedDuringRender = Math.Max(maxWidthUsedDuringRender,e.Graphics.MeasureString(text, Font).Width + 20);

                    e.Graphics.DrawImage(img,1,currentRowStartY);
                    e.Graphics.DrawString(text,Font,Brushes.Black,20,currentRowStartY );
                }
                
                //now draw parent string and icon on the right
                for (int i = 0; i < _matches.Count; i++)
                {
                    //get first parent that isn't one of the explicitly useless parent types (I'd rather know the Catalogue of an AggregateGraph than to know it's an under an AggregatesGraphNode)                
                    var descendancy = _activator.CoreChildProvider.GetDescendancyListIfAnyFor(_matches[i]);
                
                    object lastParent = null;
                    int H = -1;

                    if(descendancy != null)
                    {

                        lastParent = descendancy.Parents.LastOrDefault(parent => 
                            !TypesThatAreNotUsefulParents.Contains(parent.GetType())
                            &&
                            !(parent is IContainer)
                            );

                        //if it is the selected node draw the parents diagram too
                        if (i == keyboardSelectedIndex)
                            DrawDescendancyDiagram(e, _matches[i], descendancy, diagramStartX, diagramStartY,diagramWidth);
                    }

                    float currentRowStartY = DrawMatchesStartingAtY + (RowHeight*i);
                    
                    if (lastParent != null)
                    {

                        ColorMatrix cm = new ColorMatrix();
                        cm.Matrix33 = 0.55f;
                        ImageAttributes ia = new ImageAttributes();
                        ia.SetColorMatrix(cm);

                        var rect = new Rectangle(renderWidth - 20, (int)currentRowStartY, 19, 19);
                        var img = _coreIconProvider.GetImage(lastParent);

                        //draw the parents image on the right
                        e.Graphics.DrawImage(img, rect, 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, ia);

                        var horizontalSpaceAvailableToDrawTextInto = renderWidth - (maxWidthUsedDuringRender + 20); 

                        string text = ShrinkTextToFitWidth(lastParent.ToString(),horizontalSpaceAvailableToDrawTextInto,e.Graphics);
                        var spaceRequiredForCurrentText = e.Graphics.MeasureString(text, Font).Width;

                        e.Graphics.DrawString(text, Font, Brushes.DarkGray, renderWidth - (spaceRequiredForCurrentText + 20), currentRowStartY);
                    }
                }
            }
        }
        

        private void DrawDescendancyDiagram(PaintEventArgs e, IMapsDirectlyToDatabaseTable match, DescendancyList descendancy, int diagramStartX, int diagramStartY, int diagramWidth)
        {
            

            int diagramHeight = (int)(RowHeight * (descendancy.Parents.Length + 1));

            //draw diagram of descendancy 
            e.Graphics.FillRectangle(Brushes.White, diagramStartX, diagramStartY, diagramWidth, diagramHeight);

            //draw the parents
            for (int i = 0; i < descendancy.Parents.Length; i++)
            {
                var lineStartX = diagramStartX + (DiagramTabDistance*i);
                var lineStartY = diagramStartY + (RowHeight*i);

                var img = _activator.CoreIconProvider.GetImage(descendancy.Parents[i]);
                e.Graphics.DrawImage(img, lineStartX, lineStartY);
                e.Graphics.DrawString(descendancy.Parents[i].ToString(), Font, Brushes.Black, lineStartX + 21, lineStartY);

                if (i > 0)
                    DrawTreeNodeIsChildOfBlueLines(e, lineStartX, lineStartY);
            }

            //now draw the last object
            var lastLineStartX = diagramStartX + (DiagramTabDistance * descendancy.Parents.Length);
            var lastLineStartY = diagramStartY + (diagramHeight - RowHeight);
            
            var matchImg = _activator.CoreIconProvider.GetImage(match);
            e.Graphics.DrawImage(matchImg, lastLineStartX, lastLineStartY);
            e.Graphics.DrawString(match.ToString(), Font, Brushes.Black, lastLineStartX + 21, lastLineStartY);

            _diagramBottom = diagramStartY + diagramHeight;

            DrawTreeNodeIsChildOfBlueLines(e, lastLineStartX, lastLineStartY);

        }

        private static void DrawTreeNodeIsChildOfBlueLines(PaintEventArgs e, int lineStartX, float lineStartY)
        {
            //draw the |_ lines
            var midPointX = lineStartX - (DiagramTabDistance/2);
            var midPointY = lineStartY + (RowHeight/2);

            //straight down
            e.Graphics.DrawLine(Pens.Blue, midPointX, lineStartY, midPointX, midPointY);
            //then across
            e.Graphics.DrawLine(Pens.Blue, midPointX, midPointY, lineStartX - 1, midPointY);
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

        private void NavigateToObjectUI_FormClosed(object sender, FormClosedEventArgs e)
        {
            _isClosed = true;
            _autoCompleteProvider.UnRegister();
        }
    }
}
