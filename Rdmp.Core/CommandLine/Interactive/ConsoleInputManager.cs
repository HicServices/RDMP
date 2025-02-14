// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FAnsi.Discovery;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandLine.Interactive.Picking;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataExport.DataExtraction;
using Rdmp.Core.DataViewing;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using Spectre.Console;

namespace Rdmp.Core.CommandLine.Interactive;

/// <summary>
/// Implementation of <see cref="IBasicActivateItems"/> that handles object selection and message notification but is <see cref="IsInteractive"/>=false and throws <see cref="InputDisallowedException"/> on any attempt to illicit user feedback
/// </summary>
public class ConsoleInputManager : BasicActivateItems
{
    /// <inheritdoc/>
    public override bool IsInteractive => !DisallowInput;

    /// <summary>
    /// Set to true to throw on any blocking input methods (e.g. <see cref="TypeText"/>)
    /// </summary>
    public bool DisallowInput { get; set; }

    /// <summary>
    /// Creates a new instance connected to the provided RDMP platform databases
    /// </summary>
    /// <param name="repositoryLocator">The databases to connect to</param>
    /// <param name="globalErrorCheckNotifier">The global error provider for non fatal issues</param>
    public ConsoleInputManager(IRDMPPlatformRepositoryServiceLocator repositoryLocator,
        ICheckNotifier globalErrorCheckNotifier) : base(repositoryLocator, globalErrorCheckNotifier)
    {
    }

    public override void Show(string title, string message)
    {
        Console.WriteLine(message);
    }

    public override void ShowWarning(string message)
    {
        Console.WriteLine(message);
    }

    public override bool TypeText(DialogArgs args, int maxLength, string initialText, out string text,
        bool requireSaneHeaderText)
    {
        text = AnsiConsole.Prompt(
            new TextPrompt<string>(GetPromptFor(args))
                .AllowEmpty()
        );

        if (string.Equals(text, "Cancel", StringComparison.CurrentCultureIgnoreCase))
            // user does not want to type any text
            return false;

        // user typed "null" or some spaces or something
        if (text.IsBasicallyNull())
            text = null;

        // that's still an affirmative choice
        return true;
    }

    public override DiscoveredDatabase SelectDatabase(bool allowDatabaseCreation, string taskDescription)
    {
        if (DisallowInput)
            throw new InputDisallowedException($"Value required for '{taskDescription}'");

        var value = ReadLineWithAuto(new DialogArgs { WindowTitle = taskDescription }, new PickDatabase());
        return value.Database;
    }

    public override DiscoveredTable SelectTable(bool allowDatabaseCreation, string taskDescription)
    {
        if (DisallowInput)
            throw new InputDisallowedException($"Value required for '{taskDescription}'");

        var value = ReadLineWithAuto(new DialogArgs { WindowTitle = taskDescription }, new PickTable());
        return value.Table;
    }

    public override void ShowException(string errorText, Exception exception)
    {
        throw exception ?? new Exception(errorText);
    }

    public override bool SelectEnum(DialogArgs args, Type enumType, out Enum chosen)
    {
        if (DisallowInput)
            throw new InputDisallowedException($"Value required for '{args}'");

        var chosenStr = GetString(args, Enum.GetNames(enumType).ToList());
        try
        {
            chosen = (Enum)Enum.Parse(enumType, chosenStr);
        }
        catch (Exception)
        {
            Console.WriteLine(
                $"Could not parse value.  Valid Enum values are:{Environment.NewLine}{string.Join(Environment.NewLine, Enum.GetNames(enumType))}");
            throw;
        }

        return true;
    }

    public override bool SelectType(DialogArgs args, Type[] available, out Type chosen)
    {
        if (DisallowInput)
            throw new InputDisallowedException($"Value required for '{args}'");

        var chosenStr = GetString(args, available.Select(t => t.Name).ToList());

        if (string.IsNullOrWhiteSpace(chosenStr))
        {
            chosen = null;
            return false;
        }

        chosen = available.SingleOrDefault(t => t.Name.Equals(chosenStr));

        return chosen == null ? throw new Exception($"Unknown or incompatible Type '{chosen}'") : true;
    }


    public override IMapsDirectlyToDatabaseTable[] SelectMany(DialogArgs args, Type arrayElementType,
        IMapsDirectlyToDatabaseTable[] availableObjects)
    {
        var value = ReadLineWithAuto(args, new PickObjectByID(this), new PickObjectByName(this));

        var unavailable = value.DatabaseEntities.Except(availableObjects).ToArray();

        return unavailable.Any()
            ? throw new Exception(
                $"The following objects were not among the listed available objects {string.Join(",", unavailable.Select(o => o.ToString()))}")
            : value.DatabaseEntities.ToArray();
    }

    /// <summary>
    /// Displays the text described in the prompt theming <paramref name="args"/>
    /// </summary>
    /// <param name="args"></param>
    /// <param name="entryLabel"></param>
    /// <param name="pickers"></param>
    /// <exception cref="InputDisallowedException">Thrown if <see cref="DisallowInput"/> is true</exception>
    private string GetPromptFor(DialogArgs args, bool entryLabel = true, params PickObjectBase[] pickers)
    {
        if (DisallowInput)
            throw new InputDisallowedException($"Value required for '{args}'");

        var sb = new StringBuilder();

        if (!string.IsNullOrWhiteSpace(args.WindowTitle))
        {
            sb.Append(Markup.Escape(args.WindowTitle));

            if (entryLabel && !string.IsNullOrWhiteSpace(args.EntryLabel)) sb.Append(" - ");
        }

        if (entryLabel && !string.IsNullOrWhiteSpace(args.EntryLabel))
            sb.Append($"[green]{Markup.Escape(args.EntryLabel)}[/]");

        if (!string.IsNullOrWhiteSpace(args.TaskDescription))
        {
            sb.AppendLine();
            sb.Append($"[grey]{Markup.Escape(args.TaskDescription)}[/]");
        }

        foreach (var picker in pickers)
        {
            sb.AppendLine();
            sb.Append($"Format:[grey]{Markup.Escape(picker.Format)}[/]");

            if (picker.Examples.Any())
            {
                sb.AppendLine();
                sb.Append("Examples:");
                foreach (var example in picker.Examples)
                {
                    sb.AppendLine();
                    sb.Append($"[grey]{Markup.Escape(example)}[/]");
                }
            }

            sb.AppendLine();
            sb.Append(':');
        }

        return sb.ToString();
    }

    public override IMapsDirectlyToDatabaseTable SelectOne(DialogArgs args,
        IMapsDirectlyToDatabaseTable[] availableObjects)
    {
        if (DisallowInput)
            return args.AllowAutoSelect && availableObjects.Length == 1
                ? availableObjects[0]
                : throw new InputDisallowedException($"Value required for '{args}'");

        if (availableObjects.Length == 0)
            throw new Exception("No available objects found");

        //handle auto selection when there is one object
        if (availableObjects.Length == 1 && args.AllowAutoSelect)
            return availableObjects[0];

        Console.WriteLine(args.WindowTitle);

        Console.Write(args.EntryLabel);

        var value = ReadLineWithAuto(args, new PickObjectByID(this), new PickObjectByName(this));

        var chosen = value.DatabaseEntities?.SingleOrDefault();

        if (chosen == null)
            return null;

        return !availableObjects.Contains(chosen)
            ? throw new Exception("Picked object was not one of the listed available objects")
            : chosen;
    }

    public override bool SelectObject<T>(DialogArgs args, T[] available, out T selected)
    {
        for (var i = 0; i < available.Length; i++) Console.WriteLine($"{i}:{available[i]}");

        Console.Write(args.EntryLabel);

        var result = Console.ReadLine();

        if (int.TryParse(result, out var idx))
            if (idx >= 0 && idx < available.Length)
            {
                selected = available[idx];
                return true;
            }

        selected = default;
        return false;
    }

    private CommandLineObjectPickerArgumentValue ReadLineWithAuto(DialogArgs args, params PickObjectBase[] pickers)
    {
        if (DisallowInput)
            throw new InputDisallowedException("Value required");

        var line = AnsiConsole.Prompt(
            new TextPrompt<string>(
                GetPromptFor(args, true, pickers).Trim()));


        var cli = new CommandLineObjectPicker(new[] { line }, pickers);
        return cli[0];
    }

    public override DirectoryInfo SelectDirectory(string prompt)
    {
        if (DisallowInput)
            throw new InputDisallowedException($"Value required for '{prompt}'");

        var result = AnsiConsole.Prompt<string>(
            new TextPrompt<string>(
                    GetPromptFor(new DialogArgs
                    {
                        WindowTitle = "Select Directory",
                        EntryLabel = prompt
                    }))
                .AllowEmpty());

        return result.IsBasicallyNull() ? null : new DirectoryInfo(result);
    }

    public override FileInfo SelectFile(string prompt) => DisallowInput
        ? throw new InputDisallowedException($"Value required for '{prompt}'")
        : SelectFile(prompt, null, null);

    public override FileInfo SelectFile(string prompt, string patternDescription, string pattern)
    {
        if (DisallowInput)
            throw new InputDisallowedException($"Value required for '{prompt}'");

        var result = AnsiConsole.Prompt<string>(
            new TextPrompt<string>(
                    GetPromptFor(new DialogArgs
                    {
                        WindowTitle = "Select File",
                        EntryLabel = prompt
                    }))
                .AllowEmpty());

        return result.IsBasicallyNull() ? null : new FileInfo(result);
    }

    public override FileInfo[] SelectFiles(string prompt, string patternDescription, string pattern)
    {
        if (DisallowInput)
            throw new InputDisallowedException($"Value required for '{prompt}'");

        var file = AnsiConsole.Prompt<string>(
            new TextPrompt<string>(
                    GetPromptFor(new DialogArgs
                    {
                        WindowTitle = "Select File(s)",
                        TaskDescription = patternDescription,
                        EntryLabel = prompt
                    }))
                .AllowEmpty());

        if (file.IsBasicallyNull())
            return null;

        var asteriskIdx = file.IndexOf('*');

        if (asteriskIdx != -1)
        {
            var idxLastSlash =
                file.LastIndexOfAny(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar });

            if (idxLastSlash == -1 || asteriskIdx < idxLastSlash)
                throw new Exception("Wildcards are only supported at the file level");

            var searchPattern = file[(idxLastSlash + 1)..];
            var dirStr = file[..idxLastSlash];

            var dir = new DirectoryInfo(dirStr);

            return !dir.Exists
                ? throw new DirectoryNotFoundException($"Could not find directory:{dirStr}")
                : dir.GetFiles(searchPattern).ToArray();
        }

        return new[] { new FileInfo(file) };
    }


    protected override bool SelectValueTypeImpl(DialogArgs args, Type paramType, object initialValue, out object chosen)
    {
        chosen = UsefulStuff.ChangeType(AnsiConsole.Ask<string>(GetPromptFor(args)), paramType);
        return true;
    }

    public override bool YesNo(DialogArgs args, out bool chosen)
    {
        var result = GetString(args, new List<string> { "Yes", "No", "Cancel" });
        chosen = result == "Yes";
        //user made a non-cancel decision?
        return result != "Cancel" && !string.IsNullOrWhiteSpace(result);
    }

    public string GetString(DialogArgs args, List<string> options)
    {
        var chosen = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .PageSize(10)
                .Title(GetPromptFor(args))
                .AddChoices(options)
        );

        return chosen;
    }

    public override void ShowData(IViewSQLAndResultsCollection collection)
    {
        var point = collection.GetDataAccessPoint();
        var db = DataAccessPortal.ExpectDatabase(point, DataAccessContext.InternalDataProcessing);

        var sql = collection.GetSql();

        var logger = NLog.LogManager.GetCurrentClassLogger();
        logger.Trace($"About to ShowData from Query:{Environment.NewLine}{sql}");

        var toRun = new ExtractTableVerbatim(db.Server, sql, Console.OpenStandardOutput(), ",", null);
        toRun.DoExtraction();
    }

    public override void ShowLogs(ILoggedActivityRootObject rootObject)
    {
        foreach (var load in GetLogs(rootObject).OrderByDescending(l => l.StartTime))
        {
            Console.WriteLine(load.Description);
            Console.WriteLine(load.StartTime);

            Console.WriteLine($"Errors:{load.Errors.Count}");

            foreach (var error in load.Errors)
            {
                error.GetSummary(out var title, out var body, out _, out _);

                Console.WriteLine($"\t{title}");
                Console.WriteLine($"\t{body}");
            }

            Console.WriteLine("Tables Loaded:");

            foreach (var t in load.TableLoadInfos)
            {
                Console.WriteLine($"\t{t}: I={t.Inserts:N0} U={t.Updates:N0} D={t.Deletes:N0}");

                foreach (var source in t.DataSources)
                    Console.WriteLine($"\t\tSource:{source.Source}");
            }

            Console.WriteLine("Progress:");

            foreach (var p in load.Progress) Console.WriteLine($"\t{p.Date} {p.Description}");
        }
    }

    public override void ShowGraph(AggregateConfiguration aggregate)
    {
        ShowData(new ViewAggregateExtractUICollection(aggregate));
    }

    public override bool SelectObjects<T>(DialogArgs args, T[] available, out T[] selected)
    {
        if (available.Length == 0)
        {
            selected = Array.Empty<T>();
            return true;
        }

        for (var i = 0; i < available.Length; i++) Console.WriteLine($"{i}:{available[i]}");

        var result = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(result))
        {
            // selecting none is a valid user selection
            selected = Array.Empty<T>();
            return true;
        }

        var selectIdx = result.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();

        selected = available.Where((e, idx) => selectIdx.Contains(idx)).ToArray();
        return true;
    }

    public override void LaunchSubprocess(ProcessStartInfo startInfo)
    {
        throw new NotSupportedException();
    }

    public override void Wait(string title, Task task, CancellationTokenSource cts)
    {
        AnsiConsole.Status()
            .Spinner(Spinner.Known.Star)
            .Start(title, ctx =>
                base.Wait(title, task, cts)
            );
    }

    public override void ShowData(DataTable collection)
    {
        var tbl = new Table();

        foreach (DataColumn c in collection.Columns) tbl.AddColumn(c.ColumnName);

        foreach (DataRow row in collection.Rows) tbl.AddRow(row.ItemArray.Select(i => i.ToString()).ToArray());
        AnsiConsole.Write(tbl);
    }
}