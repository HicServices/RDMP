// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Text;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.CommandExecution;

/// <summary>
///     Cross UI platform (winforms, console, terminal gui) arguments for describing the theming and text
///     that should appear in a modal 'select something' style operation in <see cref="IBasicActivateItems" />
///     (the user interface abstraction layer)
/// </summary>
public class DialogArgs
{
    /// <summary>
    ///     What text should appear in the window area of the dialog or the initial
    ///     header text (in the case of console output)
    /// </summary>
    public string WindowTitle { get; set; }

    /// <summary>
    ///     Text that indicates what the user should be doing (i.e. user help to
    ///     remind them what is going on)
    /// </summary>
    public string TaskDescription { get; set; }

    /// <summary>
    ///     Used to indicate level of severity assocaited with the <see cref="TaskDescription" />
    /// </summary>
    public ProgressEventType DesciptionSeverity { get; set; } = ProgressEventType.Information;

    /// <summary>
    ///     The final line of text before user entered input e.g. the label on
    ///     a text box in which the user must enter the choice
    /// </summary>
    public string EntryLabel { get; set; }

    /// <summary>
    ///     If there is only one valid choice can this be automatically selected without
    ///     blocking or showing any feedback to the user.
    /// </summary>
    public bool AllowAutoSelect { get; set; }

    /// <summary>
    ///     <para>
    ///         If the user interface component allows filtering/searching then you can provide
    ///         a string here which will indicate what the initial search text should be set to
    ///         (if any).
    ///     </para>
    ///     <para>
    ///         This is the value for inside the search text not the caption.
    ///     </para>
    /// </summary>
    public string InitialSearchText { get; set; }

    /// <summary>
    ///     If asking the user to select RDMP database objects then this should be the initially
    ///     ticked set of objects in the dialog.
    /// </summary>
    public IMapsDirectlyToDatabaseTable[] InitialObjectSelection { get; set; }

    /// <summary>
    ///     If choosing Null is a valid option for the user then set this to true.
    ///     Defaults to false.
    /// </summary>
    public bool AllowSelectingNull { get; set; }

    /// <summary>
    ///     If <see cref="InitialSearchText" /> is supported by the dialog and autocomplete/history
    ///     is supported by the user interface then this can be set to a unique GUID that identifies
    ///     the source call site of the dialog e.g. find.  Once set future calls from the same site
    ///     should respect the same search text history
    /// </summary>
    public Guid? InitialSearchTextGuid { get; set; }

    /// <summary>
    ///     If this instance is being sent to the find dialog then set to true otherwise leave false
    /// </summary>
    public bool IsFind { get; set; }

    public override string ToString()
    {
        var sb = new StringBuilder();

        if (!string.IsNullOrEmpty(WindowTitle))
            sb.AppendLine($"Title:{WindowTitle}");

        if (!string.IsNullOrEmpty(TaskDescription))
            sb.AppendLine($"Task:{TaskDescription}");

        if (!string.IsNullOrEmpty(EntryLabel))
            sb.AppendLine($"Label:{EntryLabel}");

        return sb.Length == 0 ? "Undefined DialogArgs" : sb.ToString();
    }
}