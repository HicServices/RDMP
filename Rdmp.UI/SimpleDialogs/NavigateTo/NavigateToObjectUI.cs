// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Attributes;
using Rdmp.Core;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Icons.IconOverlays;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Providers;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using Rdmp.UI.Theme;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableLibraryCode.Settings;

namespace Rdmp.UI.SimpleDialogs.NavigateTo
{
    /// <summary>
    /// Allows you to search all objects in your database and rapidly select 1 which will be shown via the Emphasis system.
    /// </summary>
    public partial class NavigateToObjectUI : RDMPForm
    {
        private readonly Dictionary<IMapsDirectlyToDatabaseTable, DescendancyList> _searchables;
        private ICoreIconProvider _coreIconProvider;
        private FavouritesProvider _favouriteProvider;
        
        private const int MaxMatches = 30;
        private List<IMapsDirectlyToDatabaseTable> _matches;
        private object oMatches = new object();

        //drawing
        private int keyboardSelectedIndex = 0;
        private int mouseSelectedIndex = 0;

        private Color keyboardSelectionColor = Color.FromArgb(210, 230, 255);
        private Color mouseSelectionColor = Color.FromArgb(230, 245, 251);

        private const float DrawMatchesStartingAtY = 50;
        private const float RowHeight = 20;

        private const int DiagramTabDistance = 20;

        private Bitmap _magnifier;
        
        private Task _lastFetchTask = null;
        private CancellationTokenSource _lastCancellationToken;
        private Type[] _types;
        private HashSet<string> _typeNames;

        /// <summary>
        /// Object types that appear in the task bar as filterable types
        /// </summary>
        private Dictionary<Type,RDMPCollection> EasyFilterTypesAndAssociatedCollections = new Dictionary<Type, RDMPCollection>()
        {
            {typeof (Catalogue),RDMPCollection.Catalogue},
            {typeof (CatalogueItem),RDMPCollection.Catalogue},
            {typeof (SupportingDocument),RDMPCollection.Catalogue},
            {typeof (Project),RDMPCollection.DataExport},
            {typeof (ExtractionConfiguration),RDMPCollection.DataExport},
            {typeof (ExtractableCohort),RDMPCollection.SavedCohorts},
            {typeof (CohortIdentificationConfiguration),RDMPCollection.Cohort},
            {typeof (TableInfo),RDMPCollection.Tables},
            {typeof (ColumnInfo),RDMPCollection.Tables},
            {typeof (LoadMetadata),RDMPCollection.DataLoad},
        };


        /// <summary>
        /// Identifies which Types are checked by default when the NavigateToObjectUI is shown when the given RDMPCollection has focus
        /// </summary>
        public Dictionary<RDMPCollection, Type[]> StartingEasyFilters
            = new Dictionary<RDMPCollection, Type[]>()
            {
                {RDMPCollection.Catalogue, new[] {typeof (Catalogue)}},
                {RDMPCollection.Cohort, new[] {typeof (CohortIdentificationConfiguration)}},
                {RDMPCollection.DataExport, new[] {typeof (Project), typeof (ExtractionConfiguration)}},
                {RDMPCollection.DataLoad, new[] {typeof (LoadMetadata)}},
                {RDMPCollection.SavedCohorts, new[] {typeof (ExtractableCohort)}},
                {RDMPCollection.Tables, new[] {typeof (TableInfo)}},
                {RDMPCollection.None,new []{typeof(SupportingDocument),typeof(CatalogueItem)}} //Add all other Type checkboxes here so that they are recognised as Typenames
};

        private bool _isClosed;

        private List<Type> showOnlyTypes = new List<Type>();
        private AttributePropertyFinder<UsefulPropertyAttribute> _usefulPropertyFinder;
        private Type _alwaysFilterOn;
        private ToolStripTextBox _lblId;

        /// <summary>
        /// The action to perform when the form closes with an object selected (defaults to Emphasise)
        /// </summary>
        public Action<IMapsDirectlyToDatabaseTable> CompletionAction { get; set; }

        /// <summary>
        /// Hides the Type selection toggle buttons and forces results to only appear matching the given Type
        /// </summary>
        public Type AlwaysFilterOn
        {
            get => _alwaysFilterOn;
            set
            {
                if(value != null)
                    Controls.Remove(toolStrip1);
                else
                    Controls.Add(toolStrip1);

                _alwaysFilterOn = value;
                tbFind_TextChanged(this, null);
            }
        }

        public NavigateToObjectUI(IActivateItems activator, string initialSearchQuery = null,RDMPCollection focusedCollection = RDMPCollection.None):base(activator)
        {
            _coreIconProvider = activator.CoreIconProvider;
            _favouriteProvider = Activator.FavouritesProvider;
            _magnifier = FamFamFamIcons.magnifier;
            InitializeComponent();

            CompletionAction = Emphasise;

            activator.Theme.ApplyTo(toolStrip1);

            _searchables = Activator.CoreChildProvider.GetAllSearchables();
            
            _usefulPropertyFinder = new AttributePropertyFinder<UsefulPropertyAttribute>(_searchables.Keys);

            textBox1.Focus();
            textBox1.Text = initialSearchQuery;
            
            textBox1.TextChanged += tbFind_TextChanged;
            textBox1.KeyUp += _scintilla_KeyUp;
            textBox1.KeyPress += (s, e) =>
            {
                //prevents windows 'bong' noise when you hit enter
                if (e.KeyChar == (int) Keys.Enter)
                    e.Handled = true;
            };

            FetchMatches(initialSearchQuery,CancellationToken.None);
            StartPosition = FormStartPosition.CenterScreen;
            DoubleBuffered = true;
            
            _types = _searchables.Keys.Select(k => k.GetType()).Distinct().ToArray();
            _typeNames = new HashSet<string>(_types.Select(t => t.Name));

            foreach (Type t in StartingEasyFilters.SelectMany(v=>v.Value))
            {
                if (!_typeNames.Contains(t.Name))
                    _typeNames.Add(t.Name);
            }

            Type[] startingFilters = null;

            if (focusedCollection != RDMPCollection.None && StartingEasyFilters.ContainsKey(focusedCollection))
                startingFilters = StartingEasyFilters[focusedCollection];
            
            BackColorProvider backColorProvider = new BackColorProvider();

            foreach (Type t in EasyFilterTypesAndAssociatedCollections.Keys)
            {
                var b = new ToolStripButton();
                b.Image = activator.CoreIconProvider.GetImage(t);
                b.CheckOnClick = true;
                b.Tag = t;
                b.DisplayStyle = ToolStripItemDisplayStyle.Image;

                string shortCode = SearchablesMatchScorer.ShortCodes.Single(kvp=>kvp.Value == t).Key;

                b.Text = $"{t.Name} ({shortCode})";
                b.CheckedChanged += CollectionCheckedChanged;
                b.Checked = startingFilters != null && startingFilters.Contains(t);

                b.BackgroundImage = backColorProvider.GetBackgroundImage(b.Size, EasyFilterTypesAndAssociatedCollections[t]);

                toolStrip1.Items.Add(b);
            }

            toolStrip1.Items.Add(new ToolStripLabel("ID:"));
            toolStrip1.Items.Add(_lblId = new ToolStripTextBox());
            _lblId.TextChanged += tbFind_TextChanged;

            AddUserSettingCheckbox(() => UserSettings.ShowInternalCatalogues, (v) => UserSettings.ShowInternalCatalogues = v,"I", "Include Internal");
            AddUserSettingCheckbox(() => UserSettings.ShowDeprecatedCatalogues, (v) => UserSettings.ShowDeprecatedCatalogues = v,"D", "Include Deprecated");
            AddUserSettingCheckbox(() => UserSettings.ShowColdStorageCatalogues, (v) => UserSettings.ShowColdStorageCatalogues = v,"C", "Include Cold Storage");
            AddUserSettingCheckbox(() => UserSettings.ShowProjectSpecificCatalogues, (v) => UserSettings.ShowProjectSpecificCatalogues = v, "P", "Include Project Specific");
            AddUserSettingCheckbox(() => UserSettings.ShowNonExtractableCatalogues, (v) => UserSettings.ShowNonExtractableCatalogues = v,"E", "Include Extractable");

        }

        private void AddUserSettingCheckbox(Func<bool> getter, Action<bool> setter, string name,string toolTip)
        {
            var b = new ToolStripButton(name);
            b.CheckOnClick = true;
            b.ToolTipText = toolTip;
            b.DisplayStyle = ToolStripItemDisplayStyle.Text;
            b.Checked = getter();
            b.CheckedChanged += (s, e) =>
            {
                setter(b.Checked);

                //refresh the objects showing
                tbFind_TextChanged(null, null);
            };

            toolStrip1.Items.Add(b);
        }

        private void CollectionCheckedChanged(object sender, EventArgs e)
        {
            var button = (ToolStripButton) sender;

            var togglingType = (Type) button.Tag;

            if (button.Checked)
                showOnlyTypes.Add(togglingType);
            else
                showOnlyTypes.Remove(togglingType);

            //refresh the objects showing
            tbFind_TextChanged(null, null);
        }

        void _scintilla_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up)
            {
                e.Handled = true;
                textBox1.SelectionStart = textBox1.Text.Length;
                textBox1.SelectionLength = 0;
                MoveSelectionUp();
            }

            if (e.KeyCode == Keys.Down)
            {
                e.Handled = true;
                textBox1.SelectionStart = textBox1.Text.Length;
                textBox1.SelectionLength = 0;
                MoveSelectionDown();
            }

            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                PerformCompletionAction(keyboardSelectedIndex);
            }
            
            if (e.KeyCode == Keys.Escape)
            {
                e.Handled = true;
                Close();
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
           base.OnMouseClick(e);

            if(e.Y<= DrawMatchesStartingAtY || e.Y > (RowHeight * MaxMatches )+ DrawMatchesStartingAtY)
                return;

            PerformCompletionAction(RowIndexFromPoint(e.X, e.Y));
        }
        
        private void PerformCompletionAction(int indexToSelect)
        {
            lock (oMatches)
            {
                if (indexToSelect >= _matches.Count)
                    return;

                Close();
                CompletionAction(_matches[indexToSelect]);
            }
        }

        private void Emphasise(IMapsDirectlyToDatabaseTable o)
        {
            Activator.RequestItemEmphasis(this, new EmphasiseRequest(o, 1) { Pin = UserSettings.FindShouldPin });
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
            float newHeight;

            lock (oMatches)
            {
                newHeight = _matches.Count*RowHeight;
            }
            SetClientSizeCore(ClientSize.Width,
                    Math.Max(0,(int)((newHeight) + DrawMatchesStartingAtY)));
            
        }

        private int RowIndexFromPoint(int x, int y)
        {
            y -= (int)DrawMatchesStartingAtY;

            return Math.Max(0,Math.Min(MaxMatches,(int) (y/RowHeight)));
        }

        private void MoveSelectionDown()
        {
            lock (oMatches)
            {
                keyboardSelectedIndex = Math.Min(_matches.Count-1, //don't go above the number matches returned
                    Math.Min(MaxMatches - 1, //don't go above the max number of matches 
                        keyboardSelectedIndex + 1));
            }

            Invalidate();
        }

        private void MoveSelectionUp()
        {
            lock (oMatches)
            {
                keyboardSelectedIndex = Math.Min(_matches.Count - 1,  //if text has been typed then selectedIndex could be higher than the number of matches so set that as a roof
                   Math.Max(0, //also don't go below 0
                       keyboardSelectedIndex - 1)); 
            }
            
            Invalidate();
        }


        protected override void OnDeactivate(EventArgs e)
        {
            this.Close();
        }


        private void tbFind_TextChanged(object sender, EventArgs e)
        {
            //cancel the last execution if it has not completed yet
            if (_lastFetchTask != null && !_lastFetchTask.IsCompleted)
                _lastCancellationToken.Cancel();

            _lastCancellationToken = new CancellationTokenSource();

            var toFind = textBox1.Text;

            _lastFetchTask = Task.Run(() => FetchMatches(toFind, _lastCancellationToken.Token))
                .ContinueWith(
                    (s) =>
                    {
                        if (_isClosed)
                            return;
                        
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
                    }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void FetchMatches(string text, CancellationToken cancellationToken)
        {
            var scorer = new SearchablesMatchScorer();
            scorer.RespectUserSettings = true;
            scorer.TypeNames = _typeNames;
            scorer.BumpMatches = Activator.HistoryProvider.History.Select(h=>h.Object).ToList();
            
            if(_lblId != null && int.TryParse(_lblId.Text,out int requireId))
                scorer.ID = requireId;

            if(AlwaysFilterOn != null)
                showOnlyTypes = new List<Type>(new []{AlwaysFilterOn});

            var scores = scorer.ScoreMatches(_searchables, text, cancellationToken,showOnlyTypes);

            if (scores == null)
                return;
            lock (oMatches)
            {
                _matches =
                    scores
                        .Where(score => score.Value > 0)
                        .OrderByDescending(score => score.Value)
                        .ThenByDescending(id=>id.Key.Key.ID) //favour newer objects over ties
                        .Take(MaxMatches)
                        .Select(score => score.Key.Key)
                        .ToList();
            }
        }

        

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            float maxWidthUsedDuringRender = 0;
            int renderWidth = panel1.Right;
            
            lock (oMatches)
            {
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

                        float textRight = e.Graphics.MeasureString(text, Font).Width + 20;
                        //record how wide it is so we know how much space is left to draw parents
                        maxWidthUsedDuringRender = Math.Max(maxWidthUsedDuringRender,textRight);

                        e.Graphics.DrawImage(img,1,currentRowStartY);
                        e.Graphics.DrawString(text,Font,Brushes.Black,20,currentRowStartY );

                        string extraText =" " + GetUsefulProperties(_matches[i]);
                        if (!string.IsNullOrWhiteSpace(extraText))
                        {
                            e.Graphics.DrawString(extraText, Font, Brushes.Gray, textRight, currentRowStartY);
                            float extraTextRight = textRight + e.Graphics.MeasureString(extraText, Font).Width;
                            maxWidthUsedDuringRender = Math.Max(maxWidthUsedDuringRender, extraTextRight);
                        }
                    }
                
                    //now draw parent string and icon on the right
                    for (int i = 0; i < _matches.Count; i++)
                    {
                        //get first parent that isn't one of the explicitly useless parent types (I'd rather know the Catalogue of an AggregateGraph than to know it's an under an AggregatesGraphNode)                
                        var descendancy = Activator.CoreChildProvider.GetDescendancyListIfAnyFor(_matches[i]);

                        object lastParent = descendancy?.GetMostDescriptiveParent();
                        
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
        }

        /// <summary>
        /// Returns the text drawn for the object e.g. ToString() + (UsefulProperty)
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        private string GetUsefulProperties(IMapsDirectlyToDatabaseTable m)
        {
            StringBuilder sb = new StringBuilder();

            var p = _usefulPropertyFinder.GetProperties(m).ToArray();

            if (p.Length == 0)
                return null;

            sb.Append("(");

            foreach (var propertyInfo in p)
            {
                var attr = _usefulPropertyFinder.GetAttribute(propertyInfo);

                var key = string.IsNullOrWhiteSpace(attr.DisplayName) ? propertyInfo.Name : attr.DisplayName;
                var val = propertyInfo.GetValue(m);
                sb.Append(key + "='" + val + "' ");
            }
            
            sb.Append(")");

            return sb.ToString();
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
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            AdjustHeight();
        }
    }
}
