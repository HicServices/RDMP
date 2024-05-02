// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using CsvHelper;
using Rdmp.Core.DataFlowPipeline.Requirements;
using Rdmp.Core.DataLoad.Modules.Exceptions;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Modules.DataFlowSources.SubComponents;

/// <summary>
///     This class is a sub component of <see cref="DelimitedFlatFileDataFlowSource" />, it is responsible for responding
///     to errors processing the file
///     being loaded according to the <see cref="BadDataHandlingStrategy" />. It also includes settings for how to respond
///     to empty files.
/// </summary>
public class FlatFileEventHandlers
{
    private readonly FlatFileToLoad _fileToLoad;
    private readonly FlatFileToDataTablePusher _dataPusher;
    private readonly bool _throwOnEmptyFiles;
    private readonly BadDataHandlingStrategy _strategy;
    private readonly IDataLoadEventListener _listener;
    private int _maximumErrorsToReport;
    private readonly bool _ignoreBadDataEvents;

    /// <summary>
    ///     File where we put error rows
    /// </summary>
    public FileInfo DivertErrorsFile;

    public FlatFileEventHandlers(FlatFileToLoad fileToLoad, FlatFileToDataTablePusher dataPusher,
        bool throwOnEmptyFiles, BadDataHandlingStrategy strategy, IDataLoadEventListener listener,
        int maximumErrorsToReport, bool ignoreBadDataEvents)
    {
        _fileToLoad = fileToLoad;
        _dataPusher = dataPusher;
        _throwOnEmptyFiles = throwOnEmptyFiles;
        _strategy = strategy;
        _listener = listener;
        _maximumErrorsToReport = maximumErrorsToReport;
        _ignoreBadDataEvents = ignoreBadDataEvents;
    }

    public void FileIsEmpty()
    {
        if (_throwOnEmptyFiles)
            throw new FlatFileLoadException($"File {_fileToLoad} is empty");

        _listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning, $"File {_fileToLoad} is empty"));
    }

    public bool ReadingExceptionOccurred(ReadingExceptionOccurredArgs args)
    {
        var line = new FlatFileLine(args.Exception.Context);

        switch (_strategy)
        {
            case BadDataHandlingStrategy.IgnoreRows:
                if (_maximumErrorsToReport-- > 0)
                    _listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning,
                        $"Ignored ReadingException on {line.GetLineDescription()}", args.Exception));

                //move to next line
                _dataPusher.BadLines.Add(args.Exception.Context.Parser.RawRow);

                break;
            case BadDataHandlingStrategy.DivertRows:

                DivertErrorRow(new FlatFileLine(args.Exception.Context), args.Exception);
                break;

            case BadDataHandlingStrategy.ThrowException:
                throw new FlatFileLoadException($"Bad data found on li{line.GetLineDescription()}", args.Exception);

            default:
                throw new ArgumentOutOfRangeException();
        }

        //todo should this return true or false? not clear, this was an API change in CSV.
        return true;
    }

    public void BadDataFound(FlatFileLine line, bool isFromCsvHelper = false)
    {
        if (_ignoreBadDataEvents && isFromCsvHelper)
            if (_maximumErrorsToReport-- > 0)
            {
                _listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning,
                    $"Ignoring CSVHelper internal bad data warning:{line.GetLineDescription()}"));
                return;
            }

        // if we have seen this bad line already
        if (_dataPusher.BadLines.Contains(line.LineNumber))
            return;

        switch (_strategy)
        {
            case BadDataHandlingStrategy.IgnoreRows:

                if (_maximumErrorsToReport-- > 0)
                    _listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning,
                        $"Ignored BadData on {line.GetLineDescription()}"));

                //move to next line
                _dataPusher.BadLines.Add(line.LineNumber);

                break;
            case BadDataHandlingStrategy.DivertRows:
                DivertErrorRow(line, null);
                break;

            case BadDataHandlingStrategy.ThrowException:
                throw new FlatFileLoadException($"Bad data found on {line.GetLineDescription()}");


            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void DivertErrorRow(FlatFileLine line, Exception ex)
    {
        if (DivertErrorsFile == null)
        {
            DivertErrorsFile = new FileInfo(Path.Combine(_fileToLoad.File.Directory.FullName,
                $"{Path.GetFileNameWithoutExtension(_fileToLoad.File.Name)}_Errors.txt"));

            //delete any old version
            if (DivertErrorsFile.Exists)
                DivertErrorsFile.Delete();
        }

        if (_maximumErrorsToReport-- > 0)
            _listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning,
                $"Diverting Error on {line.GetLineDescription()} to '{DivertErrorsFile.FullName}'", ex));

        File.AppendAllText(DivertErrorsFile.FullName, line.RawRecord);

        //move to next line
        _dataPusher.BadLines.Add(line.LineNumber);
    }
}