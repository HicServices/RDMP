using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Win32;
using ReusableLibraryCode.Settings;
using ReusableUIComponents.Settings;

namespace ReusableUIComponents
{
    /// <summary>
    /// Maintains lists of recently typed things into text boxes etc, use HostControl to have this class setup all the autocomplete and monitor .Leave events for self population
    /// Once you call HostControl then that is you done, this class does the rest.
    /// </summary>
    public class RecentHistoryOfControls
    {
        private readonly Guid _controlGuid;
        private  HashSet<string> _recentValues;

        public RecentHistoryOfControls(TextBox c, Guid controlGuid):this(controlGuid)
        {
            var vals = new AutoCompleteStringCollection();
            vals.AddRange(_recentValues.ToArray());

            c.AutoCompleteCustomSource = vals;
            c.AutoCompleteSource = AutoCompleteSource.CustomSource;
            c.AutoCompleteMode = AutoCompleteMode.Suggest;
            c.Leave += (sender, args) => AddResult(c.Text);
        }

        public RecentHistoryOfControls(ComboBox c, Guid controlGuid):this(controlGuid)
        {
            var vals = new AutoCompleteStringCollection();
            vals.AddRange(_recentValues.ToArray());

            c.AutoCompleteCustomSource = vals;

            if (c.DropDownStyle == ComboBoxStyle.DropDownList)
            {
                c.SelectedIndexChanged += (sender, args) => AddResult(c.Text);
            }
            else
            {
                c.AutoCompleteMode = AutoCompleteMode.Suggest;
                c.AutoCompleteSource = AutoCompleteSource.CustomSource;
                c.Leave += (sender, args) => AddResult(c.Text);
            }
        }

        private RecentHistoryOfControls(Guid controlGuid)
        {
            _controlGuid = controlGuid;
            _recentValues = new HashSet<string>(UserSettings.GetHistoryForControl(controlGuid));
        }

        public void AddResult( string value,bool save = true)
        {
            _recentValues.Add(value);
            if (save)
                Save();
        }
        
        public void Clear()
        {
            //clear the selected key only
            _recentValues.Clear();
            Save();
        }
        private void Save()
        {
            UserSettings.SetHistoryForControl(_controlGuid, _recentValues);
        }

        public void SetValueToMostRecentlySavedValue(TextBox c)
        {
            if (c.AutoCompleteCustomSource.Count > 0)
                c.Text = c.AutoCompleteCustomSource[c.AutoCompleteCustomSource.Count - 1]; //set the current text to the last used text
        }
        public void SetValueToMostRecentlySavedValue(ComboBox c)
        {
            if (c.AutoCompleteCustomSource.Count > 0)
                c.Text = c.AutoCompleteCustomSource[c.AutoCompleteCustomSource.Count - 1]; //set the current text to the last used text
        }

        public void AddHistoryAsItemsToComboBox(ComboBox c)
        {
            if (c.AutoCompleteCustomSource.Count > 0)
                foreach (string s in c.AutoCompleteCustomSource)
                    c.Items.Add(s);
        }
    }
}
