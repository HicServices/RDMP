// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;
using Rdmp.Core.Curation.Data.Dashboarding;
using Rdmp.UI;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Refreshing;
using Rdmp.UI.SimpleDialogs;
using WeifenLuo.WinFormsUI.Docking;

namespace ResearchDataManagementPlatform.WindowManagement.ContentWindowTracking.Persistence;

/// <summary>
/// TECHNICAL: Base class for all dockable tabs that host a single control
/// </summary>
[System.ComponentModel.DesignerCategory("")]
[TechnicalUI]
public class RDMPSingleControlTab : DockContent, IRefreshBusSubscriber
{
    /// <summary>
    /// The control hosted on this tab
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Control Control { get; protected set; }

    public const string BasicPrefix = "BASIC";

    protected RDMPSingleControlTab(RefreshBus refreshBus, string refreshObject)
    {
        refreshBus.Subscribe(this, refreshObject);
        FormClosed += (s, e) => refreshBus.Unsubscribe(this, refreshObject);
    }

    /// <summary>
    /// Creates instance and sets <see cref="Control"/> to <paramref name="c"/>.  You
    /// will still need to add and Dock the control etc yourself
    /// </summary>
    /// <param name="refreshBus"></param>
    /// <param name="c"></param>
    /// <param name="refreshObject"></param>
    public RDMPSingleControlTab(RefreshBus refreshBus, Control c, string refreshObject)
    {
        if (refreshObject is not null)
        {
            refreshBus.Subscribe(this, refreshObject);
            FormClosed += (s, e) => refreshBus.Unsubscribe(this, refreshObject);
        }

        Control = c;
    }

    public virtual void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
    {
    }

    public virtual void HandleUserRequestingTabRefresh(IActivateItems activator)
    {
    }

    public virtual void HandleUserRequestingEmphasis(IActivateItems activator)
    {
    }

    protected override string GetPersistString()
    {
        const char s = PersistStringHelper.Separator;
        return BasicPrefix + s + Control.GetType().FullName;
    }

    public void ShowHelp(IActivateItems activator)
    {
        var typeDocs = activator.RepositoryLocator.CatalogueRepository.CommentStore;

        var sb = new StringBuilder();

        string firstMatch = null;

        foreach (var c in Controls)
            if (typeDocs.ContainsKey(c.GetType().Name))
            {
                firstMatch ??= c.GetType().Name;

                sb.AppendLine(typeDocs.GetDocumentationIfExists(c.GetType().Name, false, true));
                sb.AppendLine();
            }

        if (sb.Length > 0)
            WideMessageBox.Show(firstMatch, sb.ToString(), Environment.StackTrace, true, firstMatch,
                WideMessageBoxTheme.Help);
    }
}