// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataFlowPipeline.Requirements;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataExport.DataRelease.Pipeline;

/// <summary>
///     Middle component for preparing the Release Folders for the Release Pipeline.
///     Some destination components will complain if this is not present!
/// </summary>
public class ReleaseFolderProvider : IPluginDataFlowComponent<ReleaseAudit>, IPipelineRequirement<Project>,
    IPipelineRequirement<ReleaseData>
{
    private Project _project;
    private ReleaseData _releaseData;
    private DirectoryInfo _releaseFolder;

    [DemandsNestedInitialization] public ReleaseFolderSettings FolderSettings { get; set; }

    public ReleaseAudit ProcessPipelineData(ReleaseAudit releaseAudit, IDataLoadEventListener listener,
        GracefulCancellationToken cancellationToken)
    {
        if (releaseAudit == null)
            return null;

        if (_releaseFolder == null)
            PrepareAndCheckReleaseFolder(new FromDataLoadEventListenerToCheckNotifier(listener));

        releaseAudit.ReleaseFolder = _releaseFolder;
        return releaseAudit;
    }

    public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
    {
    }

    public void Abort(IDataLoadEventListener listener)
    {
        listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning, "This component cannot Abort!"));
    }

    public void Check(ICheckNotifier notifier)
    {
        ((ICheckable)FolderSettings).Check(notifier);
        PrepareAndCheckReleaseFolder(notifier);
    }

    public void PreInitialize(Project value, IDataLoadEventListener listener)
    {
        _project = value;
    }

    public void PreInitialize(ReleaseData value, IDataLoadEventListener listener)
    {
        _releaseData = value;
    }

    private void PrepareAndCheckReleaseFolder(ICheckNotifier notifier)
    {
        if (FolderSettings.CustomReleaseFolder != null &&
            !string.IsNullOrWhiteSpace(FolderSettings.CustomReleaseFolder.FullName))
            _releaseFolder = FolderSettings.CustomReleaseFolder;
        else
            _releaseFolder = GetFromProjectFolder(_project);

        if (_releaseFolder.Exists && _releaseFolder.EnumerateFileSystemInfos().Any())
        {
            if (notifier.OnCheckPerformed(new CheckEventArgs(
                    $"Release folder {_releaseFolder.FullName} already exists!", CheckResult.Fail, null,
                    "Do you want to delete it? You should check the contents first.")))
                _releaseFolder.Delete(true);
            else
                return;
        }

        if (FolderSettings.CreateReleaseDirectoryIfNotFound)
            _releaseFolder.Create();
        else
            throw new Exception(
                $"Intended release directory was not found and I was forbidden to create it: {_releaseFolder.FullName}");
    }

    public DirectoryInfo GetFromProjectFolder(IProject p)
    {
        if (string.IsNullOrWhiteSpace(p.ExtractionDirectory))
            return null;

        var prefix = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var suffix = string.Empty;
        if (_releaseData is { ConfigurationsForRelease: not null } && _releaseData.ConfigurationsForRelease.Keys.Any())
        {
            var releaseTicket = _releaseData.ConfigurationsForRelease.Keys.First().ReleaseTicket;
            if (_releaseData.ConfigurationsForRelease.Keys.All(x => x.ReleaseTicket == releaseTicket))
                suffix = releaseTicket;
            else
                throw new Exception("Multiple release tickets seen, this is not allowed!");
        }

        if (string.IsNullOrWhiteSpace(suffix))
        {
            if (string.IsNullOrWhiteSpace(p.MasterTicket))
                suffix = $"{p.ID}_{p.Name}";
            else
                suffix = p.MasterTicket;
        }

        return new DirectoryInfo(Path.Combine(p.ExtractionDirectory, $"{prefix}_{suffix}"));
    }
}