// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueManager.Collections.Providers;
using CatalogueManager.ItemActivation;
using CatalogueManager.SimpleDialogs.Reports;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using ReusableUIComponents;
using ReusableUIComponents.SingleControlForms;

namespace CatalogueManager.Collections
{
    /// <summary>
    /// TECHNICAL: the base class for all collections of RDMP objects in a given toolbox.
    /// </summary>
    [TechnicalUI]
    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<RDMPCollectionUI, UserControl>))]
    public abstract class RDMPCollectionUI : RDMPCollectionUI_Design,IConsultableBeforeClosing
    {
        public RDMPCollectionCommonFunctionality CommonFunctionality { get; private set; }
        
        protected RDMPCollectionUI()
        {
            CommonFunctionality = new RDMPCollectionCommonFunctionality();
        }


        public virtual void ConsultAboutClosing(object sender, FormClosingEventArgs e)
        {
            CommonFunctionality.TearDown();
        }

        public abstract void SetItemActivator(IActivateItems activator);

    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<RDMPCollectionUI_Design, UserControl>))]
    public class RDMPCollectionUI_Design : RDMPUserControl
    {
    }
}
