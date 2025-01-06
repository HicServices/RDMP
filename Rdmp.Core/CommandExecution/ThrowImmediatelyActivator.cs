// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using FAnsi.Discovery;
using Rdmp.Core.CommandLine.Interactive;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataViewing;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.CommandExecution;

/// <summary>
/// Implementation of <see cref="IBasicActivateItems"/> that writes to console and throws
/// </summary>
public class ThrowImmediatelyActivator : BasicActivateItems
{
    public ThrowImmediatelyActivator(IRDMPPlatformRepositoryServiceLocator repositoryLocator,
        ICheckNotifier notifier = null) : base(repositoryLocator, notifier ?? ThrowImmediatelyCheckNotifier.Quiet)
    {
    }

    public override bool IsInteractive => false;

    public override bool YesNo(DialogArgs args, out bool chosen) => throw new InputDisallowedException(nameof(YesNo));

    public override DiscoveredTable SelectTable(bool allowDatabaseCreation, string taskDescription) =>
        throw new InputDisallowedException(nameof(SelectTable));

    public override IMapsDirectlyToDatabaseTable[] SelectMany(DialogArgs args, Type arrayElementType,
        IMapsDirectlyToDatabaseTable[] availableObjects) =>
        throw new InputDisallowedException(nameof(SelectMany));

    public override IMapsDirectlyToDatabaseTable SelectOne(DialogArgs args,
        IMapsDirectlyToDatabaseTable[] availableObjects) => throw new InputDisallowedException(nameof(SelectOne));

    public override bool SelectObject<T>(DialogArgs args, T[] available, out T selected) =>
        throw new InputDisallowedException(nameof(SelectObject));

    public override bool SelectObjects<T>(DialogArgs args, T[] available, out T[] selected) =>
        throw new InputDisallowedException(nameof(SelectObjects));

    public override DirectoryInfo SelectDirectory(string prompt) =>
        throw new InputDisallowedException(nameof(SelectDirectory));

    public override FileInfo SelectFile(string prompt) => throw new InputDisallowedException(nameof(SelectFile));

    public override FileInfo[] SelectFiles(string prompt, string patternDescription, string pattern) =>
        throw new InputDisallowedException(nameof(SelectFiles));

    public override void ShowData(IViewSQLAndResultsCollection collection)
    {
    }

    public override void ShowData(DataTable collection)
    {
    }

    public override void ShowGraph(AggregateConfiguration aggregate)
    {
    }

    public override void LaunchSubprocess(ProcessStartInfo startInfo) => throw new NotImplementedException();

    public override void ShowLogs(ILoggedActivityRootObject rootObject)
    {
    }

    public override FileInfo SelectFile(string prompt, string patternDescription, string pattern) =>
        throw new InputDisallowedException(nameof(SelectFile));

    public override void ShowException(string errorText, Exception exception)
    {
    }

    protected override bool
        SelectValueTypeImpl(DialogArgs args, Type paramType, object initialValue, out object chosen) =>
        throw new InputDisallowedException(nameof(SelectValueTypeImpl));

    public override void Show(string title, string message)
    {
    }

    public override bool TypeText(DialogArgs args, int maxLength, string initialText, out string text,
        bool requireSaneHeaderText)
    {
        text = null;
        return false;
    }

    public override DiscoveredDatabase SelectDatabase(bool allowDatabaseCreation, string taskDescription) =>
        throw new InputDisallowedException(nameof(SelectDatabase));

    public override bool SelectEnum(DialogArgs args, Type enumType, out Enum chosen) =>
        throw new InputDisallowedException(nameof(SelectEnum));

    public override bool SelectType(DialogArgs args, Type[] available, out Type chosen) =>
        throw new InputDisallowedException(nameof(SelectType));
}