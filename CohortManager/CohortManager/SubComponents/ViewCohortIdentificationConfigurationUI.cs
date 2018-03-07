using System;
using System.ComponentModel;
using System.Windows.Forms;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.Cohort;
using CatalogueManager.AggregationUIs.Advanced.Options;
using CatalogueManager.Collections;
using CatalogueManager.ItemActivation;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using CohortManagerLibrary.QueryBuilding;
using RDMPObjectVisualisation.Copying;
using ReusableUIComponents;
using ReusableUIComponents.ScintillaHelper;
using ScintillaNET;

namespace CohortManager.SubComponents
{
    /// <summary>
    /// Allows you to view the code that will be executed when a  Cohort Identification Configuration is executed.  This includes any cache fetches SQL.
    /// </summary>
    public partial class ViewCohortIdentificationConfigurationUI : ViewCohortIdentificationConfigurationUI_Design
    {
        private Scintilla QueryEditor;

        public ViewCohortIdentificationConfigurationUI()
        {
            InitializeComponent();

            var factory = new ScintillaTextEditorFactory();
            QueryEditor = factory.Create(new RDMPCommandFactory());
            this.Controls.Add(QueryEditor);

            AssociatedCollection = RDMPCollection.Cohort;
        }

        public override void SetDatabaseObject(IActivateItems activator, CohortIdentificationConfiguration databaseObject)
        {
            base.SetDatabaseObject(activator,databaseObject);

            QueryEditor.ReadOnly = false;

            QueryEditor.Text = new CohortQueryBuilder(databaseObject).SQL;
            
            QueryEditor.ReadOnly = true;
        }
    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<ViewCohortIdentificationConfigurationUI_Design, UserControl>))]
    public abstract class ViewCohortIdentificationConfigurationUI_Design : RDMPSingleDatabaseObjectControl<CohortIdentificationConfiguration>
    {
        
    }
}
