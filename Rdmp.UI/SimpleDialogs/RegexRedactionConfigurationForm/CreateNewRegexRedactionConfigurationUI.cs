using Rdmp.Core.Curation.DataHelper.RegexRedaction;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using System;
using System.Drawing;
using System.Text.RegularExpressions;


namespace Rdmp.UI.SimpleDialogs.RegexRedactionConfigurationForm
{
    public partial class CreateNewRegexRedactionConfigurationUI : RDMPForm
    {
        private readonly IActivateItems _activator;

        public CreateNewRegexRedactionConfigurationUI(IActivateItems activator) : base(activator)
        {
            _activator = activator;
            InitializeComponent();
            this.btnCreate.Enabled = false;
            this.tbName.TextChanged += OnChange;
            this.tbRegexPattern.TextChanged += OnChange;
            this.tbRedactionString.TextChanged += OnChange;
            this.btnCancel.Click += Cancel;
            this.btnCreate.Click += Create;
            this.lblError.Text = "";
            this.lblError.Visible = false;
        }

        private void Cancel(object sender, EventArgs e)
        {
            Close();
        }
        private void Create(object sender, EventArgs e)
        {
            var config = new RegexRedactionConfiguration(_activator.RepositoryLocator.CatalogueRepository, tbName.Text, new Regex(tbRegexPattern.Text), tbRedactionString.Text, tbDescription.Text);
            config.SaveToDatabase();
            _activator.Publish(config);
            Close();
        }
        private bool ValidRegex()
        {
            var regexString = this.tbRegexPattern.Text;
            if (string.IsNullOrWhiteSpace(regexString)) return false;
            try
            {
                Regex.Match("", regexString);
            }
            catch (ArgumentException)
            {
                return false;
            }

            return true;
        }

        private void OnChange(object sender, EventArgs e)
        {
            var validRegex = ValidRegex();
            if (validRegex)
            {
                lblError.Text = "";
                lblError.Visible = false;
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(tbRegexPattern.Text))
                {
                    lblError.Text = "Regex Pattern is invalid";
                    lblError.Visible = true;
                    lblError.ForeColor = Color.Red;
                }
            }
            if (!string.IsNullOrWhiteSpace(this.tbName.Text)
                && validRegex && !string.IsNullOrWhiteSpace(this.tbRedactionString.Text))
            {
                this.btnCreate.Enabled = true;
            }
            else
            {
                this.btnCreate.Enabled = false;
            }
        }
    }
}
