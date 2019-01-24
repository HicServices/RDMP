using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueManager.ItemActivation;
using MapsDirectlyToDatabaseTable;

namespace CatalogueManager.Rules
{
    public class RuleBasedErrorProvider
    {
        private readonly IActivateItems _activator;
        
        public RuleBasedErrorProvider(IActivateItems activator)
        {
            _activator = activator;
        }

        public void EnsureNameUnique(TextBox tbName, INamed named)
        {
            var rule = new Rule<INamed>(
                _activator,
                named,
                (o) => o.Name.Equals(tbName.Text,StringComparison.CurrentCultureIgnoreCase) && named.GetType() == o.GetType(),
                "There is already a "+named.GetType().Name+" with that Name");

            tbName.TextChanged += rule.EventHandler;
        }

        public void EnsureAcronymUnique(TextBox tbAcronym, ICatalogue catalogue)
        {
            var rule = new Rule<ICatalogue>(
                _activator, 
                catalogue,
                (o) => o.Acronym.Equals(tbAcronym.Text, StringComparison.CurrentCultureIgnoreCase),
                "There is already a Catlogue with that Acronym");

            tbAcronym.TextChanged += rule.EventHandler;
        }

        private class Rule<T> where T : IMapsDirectlyToDatabaseTable
        {
            protected readonly IActivateItems Activator;
            private readonly T _toTest;
            private readonly string _problemDescription;
            readonly ErrorProvider _errorProvider = new ErrorProvider();
            private Func<T, bool> _func;

            public Rule(IActivateItems activator, T toTest, Func<T, bool> func, string problemDescription)
            {
                Activator = activator;
                _toTest = toTest;
                _problemDescription = problemDescription;
                _func = func;
            }

            public void EventHandler(object sender, EventArgs e)
            {
                if (Activator.CoreChildProvider.GetAllSearchables().Keys.OfType<T>().Except(new[] { _toTest }).Any(_func))
                    _errorProvider.SetError(((Control)sender), _problemDescription);
                else
                    _errorProvider.Clear();
            }
        }
    }
}