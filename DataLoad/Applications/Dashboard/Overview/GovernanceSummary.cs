// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Governance;
using CatalogueManager.TestsAndSetup.ServicePropogation;

namespace Dashboard.Overview
{
    /// <summary>
    /// Provides a list of all the Catalogues for which there is at least one GovernancePeriod (See GovernancePeriodUI) configured but for which the expiry date of the most recent 
    /// GovernancePeriod is in the past (i.e. the governance period has expired).
    /// </summary>
    public partial class GovernanceSummary : RDMPUserControl
    {
        /// <summary>
        /// A dictionary of all the catalogues that were previously goverend but are no longer governed by any active governance periods.  The Key is the expird dataset the Value is the 'Lastest but still expired governance period'
        /// </summary>
        private Dictionary<Catalogue, GovernancePeriod> _expiredCatalogues;

        public GovernanceSummary()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            if(VisualStudioDesignMode)
                return;

            GovernancePeriod[] governancePeriods = RepositoryLocator.CatalogueRepository.GetAllObjects<GovernancePeriod>();

            //get all governance periods that have an end date and that end date is in the past 
            var expiredPeriods = governancePeriods.Where(g => g.EndDate != null && g.EndDate < DateTime.Now).ToArray();

            //now just because a period has expired doesn't mean that another governing agency has not taken over authority of that dataset
            var activePeriods = governancePeriods.Except(expiredPeriods).ToArray();

            //all those catalogues that are currently governed by non-expired governance periods
            var activeCatalogues = activePeriods.SelectMany(gov => gov.GovernedCatalogues).Distinct().ToArray();

            _expiredCatalogues = new Dictionary<Catalogue, GovernancePeriod>();

            //for each expired period
            foreach (GovernancePeriod governancePeriod in expiredPeriods)
            {
                //get all the expired catalogues
                foreach (Catalogue expiredCatalogue in governancePeriod.GovernedCatalogues)
                    //if it is not got a newer governance period covering it
                    if (!activeCatalogues.Contains(expiredCatalogue))
                    {
                        //if it is not yet in our dictionary
                        if (!_expiredCatalogues.ContainsKey(expiredCatalogue))
                            _expiredCatalogues.Add(expiredCatalogue, governancePeriod);//record that it has expired and when
                        else
                        {
                            //we already know it has expired but maybe this is a more recent expiry e.g.: period 2001-2005 (expired) and period 2006-2009 (expired) we should notify user it expired in 2009 not 2005
                            if (_expiredCatalogues[expiredCatalogue].EndDate < governancePeriod.EndDate)
                                _expiredCatalogues[expiredCatalogue] = governancePeriod; //we found a later (but still expired) governance period, record it in the dictionary instead.
                        }

                    }
            }

            foreach (KeyValuePair<Catalogue, GovernancePeriod> kvp in _expiredCatalogues)
            {
                ListViewItem listViewItem = listView1.Items.Add(kvp.Key.Name);

                listViewItem.SubItems.Add(kvp.Value.Name);
                listViewItem.SubItems.Add(kvp.Value.EndDate.ToString());

            }

            //magical resize!
            foreach (ColumnHeader c in listView1.Columns)
                c.Width = -2;

            base.OnLoad(e);

        }
    }
}
