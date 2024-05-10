using Rdmp.Core.Curation.Data;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.Setting;
using Rdmp.UI.ItemActivation;
using System;
using System.Collections.Generic;
using System.Data;
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
            // remember about this checkbox for later
            var prop = _settings.Where(s => s.Key == propertyName).FirstOrDefault();
            var value = false;
            if (prop is null)
            {
                prop = new Setting(_activator.RepositoryLocator.CatalogueRepository,propertyName, Convert.ToString(false));
                prop.SaveToDatabase();
            }
            value = Convert.ToBoolean(prop.Value);
            checkboxDictionary.Add(cb, prop);

            cb.Checked = value;

            // register callback
            cb.CheckedChanged += CheckboxCheckedChanged;

            // add help
            AddTooltip(cb, propertyName);
        }
    }
}
//AutoSuggestProjectNumbers