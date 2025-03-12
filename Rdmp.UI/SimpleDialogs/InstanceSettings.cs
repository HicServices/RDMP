// Copyright (c) The University of Dundee 2024-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.Setting;
using Rdmp.UI.ItemActivation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Rdmp.UI.SimpleDialogs
{
    public partial class InstanceSettings : Form
    {
        private readonly IActivateItems _activator;
        private bool _loaded;
        private Setting[] _settings;

        public InstanceSettings(IActivateItems activator)
        {
            InitializeComponent();
            _activator = activator;
            _settings = _activator.RepositoryLocator.CatalogueRepository.GetAllObjects<Setting>();

            RegisterCheckbox(cbAutoSuggestProjectNumbers, "AutoSuggestProjectNumbers");
            RegisterCheckbox(cbCohortVersioningOnCommit, "PromptForVersionOnCohortCommit");
            RegisterCheckbox(cbYesNoAll, "ToggleYestoAllNotoAlldataloadcheck");
            RegisterCheckbox(cbExtractionPipelineQuickEdit, "ExtractionPipelineQuickEdit");
            _loaded = true;
        }

        private void CheckboxCheckedChanged(object sender, EventArgs e)
        {
            if (!_loaded)
                return;

            var cb = (CheckBox)sender;
            var mappedSetting = checkboxDictionary.GetValueOrDefault(cb);
            if (mappedSetting != null)
            {
                mappedSetting.Value = Convert.ToString(cb.Checked);
                mappedSetting.SaveToDatabase();
            }
        }
        private Dictionary<CheckBox, Setting> checkboxDictionary = new();


        private void AddTooltip(Control c, string propertyName)
        {
            var helpText =
                _activator.CommentStore.GetDocumentationIfExists($"{nameof(InstanceSettings)}.{propertyName}", false);
            if (string.IsNullOrWhiteSpace(helpText)) return;

            instanceSettingsToolTips.SetToolTip(c, UsefulStuff.SplitByLength(helpText, 100));
        }

        private void RegisterCheckbox(CheckBox cb, string propertyName)
        {
            var prop = _settings.FirstOrDefault(s => s.Key == propertyName);
            if (prop is null)
            {
                prop = new Setting(_activator.RepositoryLocator.CatalogueRepository, propertyName, Convert.ToString(false));
                prop.SaveToDatabase();
            }

            var value = Convert.ToBoolean(prop.Value);
            checkboxDictionary.Add(cb, prop);

            cb.Checked = value;

            // register callback
            cb.CheckedChanged += CheckboxCheckedChanged;

            // add help
            AddTooltip(cb, propertyName);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
