using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.DataExtraction.Commands;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataFlowPipeline.Requirements;
using Rdmp.Core.ReusableLibraryCode.Annotations;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;
using YamlDotNet.Serialization;

namespace Rdmp.Core.DataFlowPipeline;

/// <summary>
/// Pipeline component designed to prevent DataTable columns containing CHIs passing through the pipeline.
/// </summary>
[Description("Crashes the pipeline if any columns are suspected of containing CHIs")]
public sealed partial class CHIColumnFinder : IPluginDataFlowComponent<DataTable>, IPipelineRequirement<IExtractCommand>, IPipelineRequirement<IBasicActivateItems>
{
    [DemandsInitialization("Component will be shut down until this date and time", DemandType = DemandType.Unspecified)]
    public DateTime? OverrideUntil { get; set; }

    [DemandsInitialization("A Yaml file that outlines which columns in which catalogues can be safely ignored")]
    public string AllowListFile { get; set; }

    private DirectoryInfo OutputFileDirectory;

    [DemandsInitialization("If non-zero, will stop searching for CHIs after it has found that many of them in the extraction.", DefaultValue = 0)]

    public int BailOutAfter { get; set; }

    [DemandsInitialization("If checked, will log a lot more information about the CHI finding process.")]

    public bool VerboseLogging { get; set; } = false;

    private bool _firstTime = true;

    private bool _isTableAlreadyNamed;
    private readonly Dictionary<string, List<string>> _allowLists = new();

    private const string RdmpAll = "RDMP_ALL";
    private readonly string _potentialChiLocationFileDescriptor = "_Potential_CHI_Locations.csv";
    private readonly string _csvColumns = "Column,Potential CHI,Value";
    private IBasicActivateItems _activator;

    public DataTable ProcessPipelineData(DataTable toProcess, IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
    {
        if (OverrideUntil.HasValue && OverrideUntil.Value > DateTime.Now)
        {
            if (_firstTime)
                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
                    $"This component is still currently being overridden until the specified date: {OverrideUntil.Value:g}"));
            _firstTime = false;
            return toProcess;
        }

        List<string> columnGreenList = new();
        if (_allowLists.TryGetValue(RdmpAll, out var _extractionSpecificAllowances))
                columnGreenList.AddRange(_extractionSpecificAllowances);
        if (_allowLists.TryGetValue(toProcess.TableName, out var _catalogueSpecificAllowances))
                columnGreenList.AddRange(_catalogueSpecificAllowances.ToList());

        var count=0;
        string fileLocation = null;
        if (OutputFileDirectory?.Exists == true)
        {
            var CHIDir = Path.Combine(OutputFileDirectory.FullName, "FoundCHIs");
            if (!Directory.Exists(CHIDir)) Directory.CreateDirectory(CHIDir);
            fileLocation = Path.Combine(CHIDir, $"{toProcess.TableName}{_potentialChiLocationFileDescriptor}");
            if (File.Exists(fileLocation) && BailOutAfter>0)
            {
                count = File.ReadLines(fileLocation).Count()-1;
                if (count > BailOutAfter)
                {
                    if (VerboseLogging)
                        listener.OnNotify(this,
                            new NotifyEventArgs(ProgressEventType.Information,
                                $"Have skipped this chunk of Catalogue {toProcess.TableName} as there is already a number of CHIs already found"));
                    return toProcess;
                }
            }
        }

        var listFile = new Lazy<StreamWriter>(() =>
            {
                var stream = fileLocation is not null ? File.AppendText(fileLocation) : null;
                if (stream?.BaseStream.Length == 0 && _csvColumns is not null)
                    stream.WriteLine(_csvColumns);
                return stream;
            },
            LazyThreadSafetyMode.ExecutionAndPublication);

        //give the data table the correct name
        if (toProcess.ExtendedProperties.ContainsKey("ProperlyNamed") && toProcess.ExtendedProperties["ProperlyNamed"]?.Equals(true) == true)
            _isTableAlreadyNamed = true;

        if (columnGreenList.Count != 0 && VerboseLogging)
            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
                $"You have chosen the following columns to be ignored: {string.Join(",", columnGreenList)}"));

        try
        {
            foreach (var col in toProcess.Columns.Cast<DataColumn>().Where(c => !columnGreenList.Contains(c.ColumnName.Trim())))
            {
                foreach (var val in toProcess.Rows.Cast<DataRow>().Select(DeRef).AsParallel().Where(ContainsValidChi))
                {
                    Interlocked.Increment(ref count);
                    if (BailOutAfter > 0 && count > BailOutAfter) break;

                    listFile.Value?.WriteLine($"{col.ColumnName},{GetPotentialCHI(val)},{val}");
                    if (VerboseLogging || string.IsNullOrWhiteSpace(fileLocation))
                    {
                        listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning,
                            $"Column {col.ColumnName} in Dataset {toProcess.TableName} appears to contain a CHI ({val})"));
                        if (!_isTableAlreadyNamed)
                            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning,
                                "DataTable has not been named. If you want to know the dataset that the error refers to please add an ExtractCatalogueMetadata to the extraction pipeline."));
                    }
                }
                var countWritten = BailOutAfter > 0 ? Math.Min(count, BailOutAfter) : count;
                if (count != 0 && VerboseLogging) listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, $"Have Written {countWritten} Potential CHIs to {fileLocation}"));

                continue;

                [NotNull]
                string DeRef([NotNull] DataRow row) => row[col].ToString() ?? "";
            }
        }
        finally
        {
            if (listFile.IsValueCreated && listFile.Value is not null)
            {
                listFile.Value?.Flush();
                listFile.Value?.Dispose();
            }
        }

        if (count>0 && OutputFileDirectory?.Exists == true)
        {
            var countWritten = BailOutAfter > 0 ? Math.Min(count, BailOutAfter) : count;
            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, $"{countWritten} CHIs have been found in your extraction. Find them in {OutputFileDirectory.FullName}"));
            if (_activator is not null)
            {
                toProcess.ExtendedProperties.Add("AlertUIAtEndOfProcess", new Tuple<string,IBasicActivateItems>($"Some CHIs have been found in your extraction for the catalogue {toProcess.TableName}. Find them in {OutputFileDirectory.FullName}.",_activator));
            }
        }
        return toProcess;
    }

    public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
    {

    }

    public void Abort(IDataLoadEventListener listener)
    {

    }

    public void Check(ICheckNotifier notifier)
    {

    }

    // True if date exists and checksum matches
    private static bool ValidBits(int d, int m, int y, int c)
    {
        c %= 11;
        if (c != 0) return false;

        return m switch
        {
            1 or 3 or 5 or 7 or 8 or 10 or 12 => d is > 0 and < 32,
            4 or 6 or 9 or 11 => d is > 0 and < 31,
            2 => d is > 0 and < 29 || (d == 29 && y % 4 == 0),
            _ => false,
        };
    }

    private enum State
    {
        End,
        Rest1, Rest2, Rest3,
        Year1, Year2,
        Month1, Month2,
        Day1, Day2,
        MaybeSpace,
        Complete
    }

    public static string GetPotentialCHI(string toCheckStr)
    {
        if (string.IsNullOrWhiteSpace(toCheckStr) || toCheckStr.Length < 9) return "";

        var state = State.End; // Start of potential CHI
        int day = 0, month = 0, year = 0, check = 0, indexOfDay = 0;
        for (var i = toCheckStr.Length - 1; i >= 0; i--)
        {
            var c = toCheckStr[i];
            var digit = c - '0';
            if (digit is < 0 or > 9)
            {
                // Non-whitespace: if we're anywhere other than maybe-space, bail.
                switch (state)
                {
                    case State.MaybeSpace:
                        state = char.IsWhiteSpace(c) ? State.Year2 : State.End;
                        break;

                    case State.Day1: // Might be a 9 digit "CHI" with leading zero removed
                    case State.Complete:
                        state = State.End;
                        if (ValidBits(day, month, year, check))
                            return toCheckStr.Substring(i + 1, 9);

                        break;

                    default:
                        state = State.End;
                        break;
                }
                continue;
            }

            // OK, we got a digit. What does it mean in the current state?
            switch (state)
            {
                case State.End:
                    check = digit;
                    state = State.Rest3;
                    break;

                case State.Rest3:
                    check += digit * 2;
                    state = State.Rest2;
                    break;

                case State.Rest2:
                    check += digit * 3;
                    state = State.Rest1;
                    break;

                case State.Rest1:
                    check += digit * 4;
                    state = State.MaybeSpace;
                    break;

                case State.MaybeSpace:
                case State.Year2:
                    check += digit * 5;
                    year = digit;
                    state = State.Year1;
                    break;

                case State.Year1:
                    check += digit * 6;
                    year += digit * 10;
                    state = State.Month2;
                    break;

                case State.Month2:
                    check += digit * 7;
                    month = digit;
                    state = State.Month1;
                    break;

                case State.Month1:
                    check += digit * 8;
                    month += digit * 10;
                    state = State.Day2;
                    break;

                case State.Day2:
                    check += digit * 9;
                    day = digit;
                    indexOfDay = i;
                    state = State.Day1;
                    break;

                case State.Day1:
                    check += digit * 10;
                    day += 10 * digit;
                    indexOfDay = i;
                    state = State.Complete;
                    break;

                case State.Complete:
                    // More than 10 digits - just keep consuming, cannot possibly be valid now.
                    day = 32;
                    break;
            }
        }
        return ValidBits(day, month, year, check) ? toCheckStr.Substring(indexOfDay, Math.Min(10,toCheckStr.Length)) : "";
    }

    private static bool ContainsValidChi([CanBeNull] object toCheck)
    {
        if (toCheck == null || toCheck == DBNull.Value)
            return false;

        var toCheckStr = toCheck.ToString();
        if (toCheckStr is null || toCheckStr.Length < 9) return false;

        var state = State.End; // Start of potential CHI
        int day = 0, month = 0, year = 0, check = 0;
        for (var i = toCheckStr.Length - 1; i >= 0; i--)
        {
            var c = toCheckStr[i];
            var digit = c - '0';
            if (digit is < 0 or > 9)
            {
                // Non-whitespace: if we're anywhere other than maybe-space, bail.
                switch (state)
                {
                    case State.MaybeSpace:
                        state = char.IsWhiteSpace(c) ? State.Year2 : State.End;
                        break;

                    case State.Day1: // Might be a 9 digit "CHI" with leading zero removed
                    case State.Complete:
                        state = State.End;
                        if (ValidBits(day, month, year, check))
                            return true;

                        break;

                    default:
                        state = State.End;
                        break;
                }
                continue;
            }

            // OK, we got a digit. What does it mean in the current state?
            switch (state)
            {
                case State.End:
                    check = digit;
                    state = State.Rest3;
                    break;

                case State.Rest3:
                    check += digit * 2;
                    state = State.Rest2;
                    break;

                case State.Rest2:
                    check += digit * 3;
                    state = State.Rest1;
                    break;

                case State.Rest1:
                    check += digit * 4;
                    state = State.MaybeSpace;
                    break;

                case State.MaybeSpace:
                case State.Year2:
                    check += digit * 5;
                    year = digit;
                    state = State.Year1;
                    break;

                case State.Year1:
                    check += digit * 6;
                    year += digit * 10;
                    state = State.Month2;
                    break;

                case State.Month2:
                    check += digit * 7;
                    month = digit;
                    state = State.Month1;
                    break;

                case State.Month1:
                    check += digit * 8;
                    month += digit * 10;
                    state = State.Day2;
                    break;

                case State.Day2:
                    check += digit * 9;
                    day = digit;
                    state = State.Day1;
                    break;

                case State.Day1:
                    check += digit * 10;
                    day += 10 * digit;
                    state = State.Complete;
                    break;

                case State.Complete:
                    // More than 10 digits - just keep consuming, cannot possibly be valid now.
                    day = 32;
                    break;
            }
        }
        return ValidBits(day, month, year, check);
    }

    public void PreInitialize(IExtractCommand value, IDataLoadEventListener listener)
    {
        if (value is not ExtractDatasetCommand edcs) return;

        OutputFileDirectory = value.GetExtractionDirectory();
        try
        {
            var hashOnReleaseColumns = edcs.Catalogue.CatalogueItems.Select(static ci => ci.ExtractionInformation)
                .Where(static ei => ei?.HashOnDataRelease == true).Select(static ei => ei.GetRuntimeName()).ToArray();

            if (!hashOnReleaseColumns.Any() && string.IsNullOrWhiteSpace(AllowListFile)) return;

            if (hashOnReleaseColumns.Length > 0)
            {
                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
                    $"Ignoring the following columns as they have been hashed on release: {string.Join(", ", hashOnReleaseColumns)}"));
            }

            if (File.Exists(AllowListFile) && _allowLists.Count == 0)
            {
                var allowListFileContent = File.ReadAllText(AllowListFile);
                var deserializer = new DeserializerBuilder().Build();
                var yamlObject = deserializer.Deserialize<Dictionary<string, List<string>>>(allowListFileContent);
                foreach (var (catalogue, columns) in yamlObject)
                {
                    _allowLists.Add(catalogue, columns);
                }
            }

            if (!hashOnReleaseColumns.Any()) return;

            if (_allowLists.TryGetValue("RDMP_ALL", out var allowAllList))
            {
                allowAllList.AddRange(hashOnReleaseColumns);
                _allowLists["RDMP_ALL"] = allowAllList;
            }
            else
            {
                _allowLists.Add("RDMP_ALL", hashOnReleaseColumns.ToList());
            }
        }
        catch (Exception e)
        {
            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning,
                $"Failed to get HashOnDataRelease columns for catalogue {edcs.Catalogue.Name} with the exception: {e.Message} Columns can be ignored manually using the Ignore Columns option", e));
        }
    }

    public void PreInitialize(IBasicActivateItems value, IDataLoadEventListener listener)
    {
        _activator = value;
    }

}