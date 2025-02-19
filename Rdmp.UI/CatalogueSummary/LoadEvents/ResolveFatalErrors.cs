// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Linq;
using Rdmp.Core.Logging;
using Rdmp.Core.Logging.PastEvents;
using Rdmp.UI.Copying;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.ScintillaHelper;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using ScintillaNET;

namespace Rdmp.UI.CatalogueSummary.LoadEvents;

/// <summary>
/// Fatal errors are crashes of the data load engine when it is attempting to load data.  If you use the RAW / STAGING / LIVE model this should almost never result in contaminating your
/// live dataset (See 'RAW, STAGING, LIVE' in UserManual.md).  If however you have lots of post load tasks or other custom functionality with knock on consequences it is possible
/// that a crash will result unforeseen consequences.  For this reason all failure messages are stored in an 'unresolved' state.  Through LoadEventsTreeView you can launch this dialog.
/// 
/// <para>This dialog lets you mark errors as 'resolved' and provide a message as to why the error happened and the steps you have taken to resolve it e.g. "Data load failed because data provider
/// has started sending 'Male' instead of 'M' for column 'Gender', I have adjusted the column width to varchar(10) to compensate and have created an issue to standardise the column values".</para>
/// </summary>
public partial class ResolveFatalErrors : RDMPForm
{
    private readonly LogManager _logManager;
    private readonly ArchivalFatalError[] _toResolve;
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Scintilla Explanation { get; set; }

    public ResolveFatalErrors(IActivateItems activator, LogManager logManager, ArchivalFatalError[] toResolve) :
        base(activator)
    {
        _logManager = logManager;
        _toResolve = toResolve;
        InitializeComponent();

        if (VisualStudioDesignMode ||
            logManager == null) //don't add the QueryEditor if we are in design time (visual studio) because it breaks
            return;

        Explanation = new ScintillaTextEditorFactory().Create(new RDMPCombineableFactory());
        Explanation.ReadOnly = false;

        //if there is only 1 explanation already recorded then we should populate the explanation textbox with this
        if (toResolve.Select(e => e.Explanation).Distinct().Count() == 1)
            Explanation.Text = toResolve.First().Explanation;

        tbFatalErrorIDs.Text = string.Join(",", toResolve.Select(f => f.ID));
        pbExplanation.Controls.Add(Explanation);
    }

    private void btnSaveAndClose_Click(object sender, EventArgs e)
    {
        var newState = DataLoadInfo.FatalErrorStates.Resolved;

        if (string.IsNullOrEmpty(Explanation.Text))
            Explanation.Text = "No Explanation";

        //resolve it in the database
        _logManager.ResolveFatalErrors(_toResolve.Select(f => f.ID).ToArray(), newState, Explanation.Text);

        //resolve it in memory
        foreach (var error in _toResolve)
            error.Explanation = Explanation.Text;

        Close();
    }
}