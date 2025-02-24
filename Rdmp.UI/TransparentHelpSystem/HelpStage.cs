// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Rdmp.UI.TransparentHelpSystem;

/// <summary>
/// Overlay message box which appears next to a given <see cref="HighlightControl"/> and describes how the user might interact
/// with it.
/// </summary>
public class HelpStage
{
    private readonly Func<bool> _moveOnWhenConditionMet;
    private readonly int _pollMilliseconds;
    public readonly Control HighlightControl;
    public readonly string HelpText;

    /// <summary>
    /// If true then HostLocationForStageBox will be ignored and the positioning of the HelpBox will be decided based on the location of the highlighted control and the
    /// surrounding available placement space on the host Form.
    /// </summary>
    public bool UseDefaultPosition { get; set; }

    public readonly Point HostLocationForStageBox;

    public string OptionButtonText;
    public HelpStage OptionDestination;

    public HelpStage Next;


    public HelpStage(Control highlightControl, string helpText, params HelpStage[] nextStagesInOrder)
    {
        HighlightControl = highlightControl;
        HelpText = helpText;
        UseDefaultPosition = true;

        HandleParams(nextStagesInOrder);
    }

    public HelpStage(Control highlightControl, string helpText, Point hostLocationForStageBox,
        params HelpStage[] nextStagesInOrder)
    {
        HighlightControl = highlightControl;
        HelpText = helpText;
        HostLocationForStageBox = hostLocationForStageBox;
        UseDefaultPosition = false;

        HandleParams(nextStagesInOrder);
    }

    public HelpStage(Control highlightControl, string helpText, Func<bool> moveOnWhenConditionMet,
        int pollMilliseconds = 300, params HelpStage[] nextStagesInOrder) : this(highlightControl, helpText,
        nextStagesInOrder)
    {
        _moveOnWhenConditionMet = moveOnWhenConditionMet;
        _pollMilliseconds = pollMilliseconds;
    }

    /// <summary>
    /// If there is moveOnWhenConditionMet set up in this HelpStage then this will start polling for the condition.  If the condition is met before cancellation true
    /// is returned (i.e. show the next stage).  Returns false if there is no moveOnCondition set up or the CancellationToken is cancelled.
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task<bool> Await(CancellationToken token)
    {
        if (_moveOnWhenConditionMet == null)
            return false;

        while (!token.IsCancellationRequested && !_moveOnWhenConditionMet())
            await Task.Delay(_pollMilliseconds, token);

        return !token.IsCancellationRequested;
    }

    private void HandleParams(HelpStage[] nextStagesInOrder)
    {
        for (var i = 0; i < nextStagesInOrder.Length; i++)
            if (i == 0)
                Next = nextStagesInOrder[i];
            else
                nextStagesInOrder[i - 1].Next = nextStagesInOrder[i];
    }

    public void SetOption(string optionButtonText, HelpStage destinationWhenOptionTaken)
    {
        OptionButtonText = optionButtonText;
        OptionDestination = destinationWhenOptionTaken;
    }

    public HelpStage SetNext(HelpStage next)
    {
        Next = next;
        return next;
    }

    public override string ToString()
    {
        var textToShow = HelpText;

        if (HelpText.Length > 47)
            textToShow = $"{HelpText[..47]}...";

        return $"Help Stage:{textToShow}";
    }
}