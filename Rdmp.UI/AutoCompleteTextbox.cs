// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

namespace CatalogueManager
{
    using System;
    // if you would like to use ArrayList insted
    using System.Collections.Generic;   // here we use Generic Type List<string>
    using System.Drawing;
    using System.Linq;
    using System.Windows.Forms;
    //from HANSHAMMEL
    //http://www.codeproject.com/Articles/251110/AutoComplete-TextBox-with-substing-search-similar


    // the component is derived from a TextBox 
    // and is therfore called TextBox, but meat is this class (AutoCompleteTextbox)
    public class AutoCompleteTextbox : TextBox
    {
        public object SelectedItem
        {
            get; private set;
        }
        public event EventHandler SelectedItemChanged;

        #region Fields

        // the ListBox used for suggestions
        private ListBox listBox;

        // string to remember a former input
        private string oldText;

        // a Panel for displaying
        private Panel panel;

        #endregion Fields

        #region Constructors

        public bool SuppressAutoComplete { get; set; }

        // the constructor
        public AutoCompleteTextbox()
            : base()
        {
            // assigning some default values
            // minimum characters to be typed before suggestions are displayed
            this.MinTypedCharacters = 2;
            // not cases sensitive
            this.CaseSensitive = false;
            // the list for our suggestions database
            // the sample dictionary en-EN.dic is stored here when form1 is loaded (see form1.Load event)
            this.AutoCompleteList = new List<object>();

            // the listbox used for suggestions
            this.listBox = new ListBox();
            this.listBox.Name = "SuggestionListBox";
            this.listBox.Font = this.Font;
            this.listBox.Visible = true;

            // the panel to hold the listbox later on
            this.panel = new Panel();
            this.panel.Visible = false;
            this.panel.Font = this.Font;
            // to be able to fit to changing sizes of the parent form
            this.panel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            // initialize with minimum size to avoid overlaping or flickering problems
            this.panel.ClientSize = new System.Drawing.Size(1, 1);
            this.panel.Name = "SuggestionPanel";
            this.panel.Padding = new System.Windows.Forms.Padding(0, 0, 0, 0);
            this.panel.Margin = new System.Windows.Forms.Padding(0, 0, 0, 0);
            this.panel.BackColor = Color.Transparent;
            this.panel.ForeColor = Color.Transparent;
            this.panel.Text = "";
            this.panel.PerformLayout();
            // add the listbox to the panel if not already done
            if (!panel.Controls.Contains(listBox))
            {
                this.panel.Controls.Add(listBox);
            }

            // make the listbox fill the panel
            this.listBox.Dock = DockStyle.Fill;
            // only one itme can be selected from the listbox
            this.listBox.SelectionMode = SelectionMode.One;
            // the events to be fired if an item is selected
            this.listBox.KeyDown += new KeyEventHandler(listBox_KeyDown);
            this.listBox.MouseClick += new MouseEventHandler(listBox_MouseClick);
            this.listBox.MouseDoubleClick += new MouseEventHandler(listBox_MouseDoubleClick);

            #region Excursus: ArrayList vs. List<string>
            // surpringly ArrayList is a little faster than List<string>
            // to use ArrayList instead, replace every 'List<string>' with 'ArrayList'
            // i will used List<string> cause it's generic type
            #endregion Excursus: ArrayList vs. List<string>
            // the list of suggestions actually displayed in the listbox
            // a subset of AutoCompleteList according to the typed in keyphrase
            this.CurrentAutoCompleteList = new List<string>();

            #region Excursus: DataSource vs. AddRange
            // using DataSource is faster than adding items (see
            // uncommented listBox.Items.AddRange method below)
            #endregion Excursus: DataSource vs. AddRange
            // Bind the CurrentAutoCompleteList as DataSource to the listbox
            listBox.DataSource = CurrentAutoCompleteList;

            // set the input to remember, which is empty so far
            oldText = this.Text;
        }

        #endregion Constructors

        #region Properties

        // the list for our suggestions database
        public List<object> AutoCompleteList
        {
            get;
            set;
        }

        // case sensitivity
        public bool CaseSensitive
        {
            get;
            set;
        }

        // minimum characters to be typed before suggestions are displayed
        public int MinTypedCharacters
        {
            get;
            set;
        }

        // the index selected in the listbox
        // maybe of intrest for other classes
        public int SelectedIndex
        {
            get
            {
                return listBox.SelectedIndex;
            }
            set
            {
                // musn't be null
                if (listBox.Items.Count != 0)
                { listBox.SelectedIndex = value; }
            }
        }

        // the actual list of currently displayed suggestions
        private List<string> CurrentAutoCompleteList
        {
            set;
            get;
        }

        // the parent Form of this control
        private Form ParentForm
        {
            get { return this.Parent.FindForm(); }
        }

        #endregion Properties

        #region Methods

        // hides the listbox
        public void HideSuggestionListBox()
        {
            if ((ParentForm != null))
            {
                // hiding the panel also hides the listbox
                panel.Hide();
                // now remove it from the ParentForm (Form1 in this example)
                if (this.ParentForm.Controls.Contains(panel))
                {
                    this.ParentForm.Controls.Remove(panel);
                }
            }
        }

        protected override void OnKeyDown(KeyEventArgs args)
        {
            // if user presses key.up
            if ((args.KeyCode == Keys.Up))
            {
                // move the selection in listbox one up
                MoveSelectionInListBox((SelectedIndex - 1));
                // work is done
                args.Handled = true;
            }
            // on key.down
            else if ((args.KeyCode == Keys.Down))
            {
                //move one down
                MoveSelectionInListBox((SelectedIndex + 1));
                args.Handled = true;
            }
            else if ((args.KeyCode == Keys.PageUp))
            {
                //move 10 up
                MoveSelectionInListBox((SelectedIndex - 10));
                args.Handled = true;
            }
            else if ((args.KeyCode == Keys.PageDown))
            {
                //move 10 down
                MoveSelectionInListBox((SelectedIndex + 10));
                args.Handled = true;
            }
            // on enter
            else if ((args.KeyCode == Keys.Enter))
            {
                // select the item in the ListBox
                SelectItem();
                args.Handled = true;
            }
            else
            {
                // work is not done, maybe the base class will process the event, so call it...
                base.OnKeyDown(args);
            }
        }

        protected override bool IsInputKey(Keys keyData)
        {
            //tab is special key, we want to preserve the Tab shifting focus but want to also select the item thats there when they press it
            if (keyData == Keys.Tab || keyData == (Keys.Shift | Keys.Tab))
            {
                SelectItem();
                return false;
            }

            return base.IsInputKey(keyData);
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            if (e.KeyChar == (decimal) Keys.Enter)
                e.Handled = true; //handled elsewhere but this stops it beeping
            else
            base.OnKeyPress(e);
        }

        // if the user leaves the TextBox, the ListBox and the panel ist hidden
        protected override void OnLostFocus(System.EventArgs e)
        {
            if (!panel.ContainsFocus)
            {
                // call the baseclass event
                base.OnLostFocus(e);
                // then hide the stuff
                this.HideSuggestionListBox();
            }
        }

        // if the input changes, call ShowSuggests()
        protected override void OnTextChanged(EventArgs args)
        {
            // avoids crashing the designer
            if (!this.DesignMode)
                ShowSuggests();

            SelectedItem = AutoCompleteList.FirstOrDefault(i => i.ToString().Equals(this.Text));
            
            base.OnTextChanged(args);
            // remember input
            oldText = this.Text;
        }

        // event for any key pressed in the ListBox
        private void listBox_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            // on enter
            if (e.KeyCode == Keys.Enter)
            {
                // select the current item
                SelectItem();
                // work done
                e.Handled = true;
            }
        }
        
        // event for MouseClick in the ListBox
        private void listBox_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            // select the current item
            SelectItem();
        }

        // event for DoubleClick in the ListBox
        private void listBox_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            // select the current item
            SelectItem();
        }

        private void MoveSelectionInListBox(int Index)
        {
            // beginning of list
            if (Index <= -1)
            { this.SelectedIndex = 0; }
            else
                // end of liste
                if (Index > (listBox.Items.Count - 1))
                {
                    SelectedIndex = (listBox.Items.Count - 1);
                }
                else
                // somewhere in the middle
                { SelectedIndex = Index; }
        }

        // selects the current item
        private bool SelectItem()
        {
            // if the ListBox is not empty
            if (((this.listBox.Items.Count > 0) && (this.SelectedIndex > -1)))
            {
                // set the Text of the TextBox to the selected item of the ListBox
                this.Text = this.listBox.SelectedItem.ToString();
                this.SelectedItem = AutoCompleteList.First(obj=>obj.ToString().Equals(this.listBox.SelectedItem.ToString()));

                EventHandler h = SelectedItemChanged;
                if (h!=null)
                    h(this, new EventArgs());

                // and hide the ListBox
                this.HideSuggestionListBox();
            }
            return true;
        }

        // shows the suggestions in ListBox beneath the TextBox
        // and fitting it into the ParentForm
        private void ShowSuggests()
        {
             if(SuppressAutoComplete)
                 return;

            // show only if MinTypedCharacters have been typed
            if (this.Text.Length >= MinTypedCharacters)
            {
                // prevent overlapping problems with other controls
                // while loading data there is nothing to draw, so suspend layout
                panel.SuspendLayout();
                // user is typing forward, char has been added at the end of the former input
                if ((this.Text.Length > 0) && (this.oldText == this.Text.Substring(0, this.Text.Length - 1)))
                {
                    //handle forward typing with refresh
                    UpdateCurrentAutoCompleteList();
                }
                // user is typing backward - char bas been removed
                else if ((this.oldText.Length > 0) && (this.Text == this.oldText.Substring(0, this.oldText.Length - 1)))
                {
                    //handle backward typing with refresh
                    UpdateCurrentAutoCompleteList();
                }
                // something within has changed
                else
                {
                    // handle other things like corrections in the middle of the input, clipboard pastes, etc. with refresh
                    UpdateCurrentAutoCompleteList();
                }

                if (((CurrentAutoCompleteList != null) && CurrentAutoCompleteList.Count > 0))
                {
                    // finally show Panel and ListBox
                    // (but after refresh to prevent drawing empty rectangles)
                    panel.Show();
                    // at the top of all controls
                    panel.BringToFront();
                    // then give the focuse back to the TextBox (this control)
                    this.Focus();
                }
                // or hide if no results
                else
                {
                    this.HideSuggestionListBox();
                }
                // prevent overlapping problems with other controls
                panel.ResumeLayout(true);
            }
            // hide if typed chars <= MinCharsTyped
            else
            {
                this.HideSuggestionListBox();
            }
        }

        // This is a timecritical part
        // Fills/ refreshed the CurrentAutoCompleteList with appropreate candidates
        private void UpdateCurrentAutoCompleteList()
        {
            // the list of suggestions has to be refreshed so clear it
            CurrentAutoCompleteList.Clear();
            // an find appropreate candidates for the new CurrentAutoCompleteList in AutoCompleteList
            foreach (string Str in AutoCompleteList.Select(s=>s.ToString()))
            {
                // be casesensitive
                if (CaseSensitive)
                {
                    // search for the substring (equal to SQL Like Command)
                    if ((Str.IndexOf(this.Text) > -1))
                    // Add candidates to new CurrentAutoCompleteList
                    { CurrentAutoCompleteList.Add(Str); }
                }
                // or ignore case
                else
                {
                    // and search for the substring (equal to SQL Like Command)
                    if (SimplifyString(Str).IndexOf(SimplifyString(this.Text.ToLower())) > -1)
                    // Add candidates to new CurrentAutoCompleteList
                    { CurrentAutoCompleteList.Add(Str); }
                }
            }
            #region Excursus: Performance measuring of Linq queries
            // This is a simple example of speedmeasurement for Linq querries
            /*
            CurrentAutoCompleteList.Clear();
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            // using Linq query seems to be slower (twice as slow)
            var query =
                from expression in this.AutoCompleteList
                where expression.ToLower().Contains(this.Text.ToLower())
                select expression;
            foreach (string searchTerm in query)
            {
                CurrentAutoCompleteList.Add(searchTerm);
            }
            stopWatch.Stop();
            // method (see below) for printing the performance values to console
            PrintStopwatch(stopWatch, "Linq - Contains");
            */
            #endregion Excursus: Performance measuring of Linq queries

            // countinue to update the ListBox - the visual part
            UpdateListBoxItems();
        }

        private string SimplifyString(string s)
        {
            return s.ToLower().Replace("[", "").Replace("]","");
        }

        // This is the most timecritical part, adding items to the ListBox
        private void UpdateListBoxItems()
        {
            #region Excursus: DataSource vs. AddRange
            /*
                    // This part is not used due to 'listBox.DataSource' approach (see constructor)
                    // left for comparison, delete for productive use
                    listBox.BeginUpdate();
                    listBox.Items.Clear();
                    //Fills the ListBox
                    listBox.Items.AddRange(this.CurrentAutoCompleteList.ToArray());
                    listBox.EndUpdate();
                    // to use this method remove the following
                    // "((CurrencyManager)listBox.BindingContext[CurrentAutoCompleteList]).Refresh();" line and
                    // "listBox.DataSource = CurrentAutoCompleteList;" line from the constructor
                    */
            #endregion Excursus: DataSource vs. AddRange

            // if there is a ParentForm
            if ((ParentForm != null))
            {
                // get its width
                panel.Width = this.Width;
                // calculate the remeining height beneath the TextBox
                panel.Height = this.ParentForm.ClientSize.Height - this.Height - this.Location.Y;
                // and the Location to use
                panel.Location = this.Location + new Size(0, this.Height);
                // Panel and ListBox have to be added to ParentForm.Controls before calling BingingContext
                if (!this.ParentForm.Controls.Contains(panel))
                {
                    // add the Panel and ListBox to the PartenFormtry {
                    try
                    {
                        this.ParentForm.Controls.Add(panel);
                    } catch
                    (ArgumentOutOfRangeException)//seems to blow up sometimes maybe when there are no suggestions or something?
                    {
                        return;
                    }
                }
                // Update the listBox manually - List<string> dosn't support change events
                // this is the DataSource approche, this is a bit tricky and may cause conflicts,
                // so in case fall back to AddRange approache (see Excursus)
                ((CurrencyManager)listBox.BindingContext[CurrentAutoCompleteList]).Refresh();
            }
        }

        #endregion Methods

        #region Other

        /*
        // only needed for performance measuring - delete for productional use
        private void PrintStopwatch(Stopwatch stopWatch, string comment)
        {
            // Get the elapsed time as a TimeSpan value.
            TimeSpan ts = stopWatch.Elapsed;
            // Format and display the TimeSpan value.
            string elapsedTime = String.Format("{0:00}h:{1:00}m:{2:00},{3:000}s \t {4}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds, comment);
            Console.WriteLine("RunTime " + elapsedTime);
        }
        */

        #endregion Other

        
    }
}