// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Text;
using System.Windows.Forms;
using Rdmp.UI;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Refreshing;
using Rdmp.UI.SimpleDialogs;


using WeifenLuo.WinFormsUI.Docking;

namespace ResearchDataManagementPlatform.WindowManagement.ContentWindowTracking.Persistence
{
    /// <summary>
    /// TECHNICAL: Base class for all dockable tabs that host a single control 
    /// </summary>
    [System.ComponentModel.DesignerCategory("")]
    [TechnicalUI]
    public abstract class RDMPSingleControlTab:DockContent,IRefreshBusSubscriber
    {
        protected RDMPSingleControlTab(RefreshBus refreshBus)
        {
            refreshBus.Subscribe(this);
            FormClosed += (s, e) => refreshBus.Unsubscribe(this);
        }

        public abstract Control GetControl();
        public abstract void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e);
        public abstract void HandleUserRequestingTabRefresh(IActivateItems activator);

        public abstract void HandleUserRequestingEmphasis(IActivateItems activator);

        public void ShowHelp(IActivateItems activator)
        {


            var typeDocs = activator.RepositoryLocator.CatalogueRepository.CommentStore;

            StringBuilder sb = new StringBuilder();

            string firstMatch = null;

            foreach (var c in Controls)
                if (typeDocs.ContainsKey(c.GetType().Name))
                {
                    if (firstMatch == null)
                        firstMatch = c.GetType().Name;

                    sb.AppendLine(typeDocs.GetDocumentationIfExists(c.GetType().Name,false,true));
                    sb.AppendLine();
                }

            if (sb.Length > 0)
                WideMessageBox.Show(firstMatch, sb.ToString(),Environment.StackTrace,  true,  firstMatch,WideMessageBoxTheme.Help);
        }
    }
}