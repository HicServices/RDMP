// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.ComponentModel;
using System.Windows.Forms;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.QueryBuilding;
using Rdmp.UI.Collections;
using Rdmp.UI.Copying;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using ReusableUIComponents;
using ReusableUIComponents.ScintillaHelper;
using ScintillaNET;

namespace Rdmp.UI.SubComponents
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
