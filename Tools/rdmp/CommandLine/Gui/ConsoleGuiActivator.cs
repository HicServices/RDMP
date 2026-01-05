// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using FAnsi.Discovery;
using NStack;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandLine.Gui.Windows;
using Rdmp.Core.CommandLine.Runners;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataViewing;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Revertable;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Terminal.Gui;

namespace Rdmp.Core.CommandLine.Gui;

internal class ConsoleGuiActivator : BasicActivateItems
{
    /// <summary>
    /// Fired when changes are made to an object (including if it has been deleted etc)
    /// </summary>
    public event Action<IMapsDirectlyToDatabaseTable> Published;

    public ConsoleGuiActivator(IRDMPPlatformRepositoryServiceLocator repositoryLocator,
        ICheckNotifier globalErrorCheckNotifier) : base(repositoryLocator, globalErrorCheckNotifier)
    {
        InteractiveDeletes = true;
    }


    protected override bool SelectValueTypeImpl(DialogArgs args, Type paramType, object initialValue, out object chosen)
    {
        var dlg = new ConsoleGuiTextDialog(args, initialValue?.ToString());
        if (dlg.ShowDialog())
        {
            if (string.IsNullOrWhiteSpace(dlg.ResultText) ||
                dlg.ResultText.Equals("null", StringComparison.CurrentCultureIgnoreCase))
            {
                chosen = null;
            }
            else
            {
                var wrappedType = Nullable.GetUnderlyingType(paramType);

                chosen = Convert.ChangeType(dlg.ResultText, wrappedType ?? paramType);
            }


            return true;
        }

        chosen = null;
        return false;
    }

    public override void Show(string title, string message)
    {
        GetDialogDimensions(out var w, out var h);

        var btn = new Button("Ok");
        btn.Clicked += () => Application.RequestStop();


        using var dlg = new Dialog(title, w, h, btn) { Modal = true };
        dlg.Add(new TextView
        {
            Width = Dim.Fill(),
            Height = Dim.Fill(1),
            Text = message.Replace("\r\n", "\n"),
            ReadOnly = true,
            AllowsTab = false,
            WordWrap = true
        });
        Application.Run(dlg, ConsoleMainWindow.ExceptionPopup);
    }

    public override bool YesNo(DialogArgs args, out bool chosen)
    {
        GetDialogDimensions(out var w, out var h);

        // don't use the full height if you're just asking a yes/no question with no big description
        h = Math.Min(5 + (args.TaskDescription?.Length ?? 0) / 20, h);

        var result = MessageBox.Query(w, h, args.WindowTitle ?? "", args.TaskDescription ?? "", "yes", "no", "cancel");
        chosen = result == 0;

        return result != 2;
    }

    private static void GetDialogDimensions(out int w, out int h)
    {
        w = Application.Top?.Frame.Width ?? 0;
        h = Application.Top?.Frame.Height ?? 0;

        w = Math.Max(64, Math.Min(80, w - 4));
        h = Math.Max(10, Math.Min(20, h - 2));
    }


    public override bool TypeText(DialogArgs args, int maxLength, string initialText, out string text,
        bool requireSaneHeaderText)
    {
        var dlg = new ConsoleGuiTextDialog(args, initialText) { MaxLength = maxLength };
        if (dlg.ShowDialog())
        {
            text = dlg.ResultText;
            return true;
        }

        text = null;
        return false;
    }

    public override void Publish(IMapsDirectlyToDatabaseTable databaseEntity)
    {
        base.Publish(databaseEntity);

        Published?.Invoke(databaseEntity);
    }

    public override DiscoveredDatabase SelectDatabase(bool allowDatabaseCreation, string taskDescription)
    {
        var dlg = new ConsoleGuiServerDatabaseTableSelector(this, taskDescription, "Ok", false);
        return dlg.ShowDialog() ? dlg.GetDiscoveredDatabase() : null;
    }

    public override DiscoveredTable SelectTable(bool allowDatabaseCreation, string taskDescription)
    {
        var dlg = new ConsoleGuiServerDatabaseTableSelector(this, taskDescription, "Ok", true);
        return dlg.ShowDialog() ? dlg.GetDiscoveredTable() : null;
    }

    public override IMapsDirectlyToDatabaseTable[] SelectMany(DialogArgs args, Type arrayElementType,
        IMapsDirectlyToDatabaseTable[] availableObjects)
    {
        var dlg = new ConsoleGuiSelectMany(this, args.WindowTitle, availableObjects);
        Application.Run(dlg, ConsoleMainWindow.ExceptionPopup);

        return dlg.ResultOk
            ? dlg.Result.Cast<IMapsDirectlyToDatabaseTable>().ToArray()
            : Array.Empty<IMapsDirectlyToDatabaseTable>();
    }

    public override IMapsDirectlyToDatabaseTable SelectOne(DialogArgs args,
        IMapsDirectlyToDatabaseTable[] availableObjects)
    {
        if (args.AllowAutoSelect && availableObjects.Length == 1)
            return availableObjects[0];

        var dlg = new ConsoleGuiSelectOne(this, availableObjects);
        return dlg.ShowDialog() ? dlg.Selected : null;
    }


    /// <inheritdoc/>
    public override bool SelectObject<T>(DialogArgs args, T[] available, out T selected)
    {
        if (args.AllowAutoSelect && available.Length == 1)
        {
            selected = available[0];
            return true;
        }

        var dlg = new ConsoleGuiBigListBox<T>(args.WindowTitle ?? "", "Ok", true, available, t => t.ToString(), true);

        if (dlg.ShowDialog())
        {
            selected = dlg.Selected;
            return true;
        }

        selected = default;
        return false;
    }

    public override bool SelectObjects<T>(DialogArgs args, T[] available, out T[] selected)
    {
        var dlg = new ConsoleGuiSelectMany(this, args.WindowTitle, available);
        Application.Run(dlg, ConsoleMainWindow.ExceptionPopup);

        selected = dlg.Result.Cast<T>().ToArray();
        return dlg.ResultOk;
    }

    public override DirectoryInfo SelectDirectory(string prompt)
    {
        var openDir = new OpenDialog(prompt, "Directory")
        {
            AllowsMultipleSelection = false,
            CanCreateDirectories = true,
            CanChooseDirectories = true,
            CanChooseFiles = false
        };

        Application.Run(openDir, ConsoleMainWindow.ExceptionPopup);

        var selected = openDir.FilePath?.ToString();

        return selected == null ? null : new DirectoryInfo(selected);
    }

    public override FileInfo SelectFile(string prompt)
    {
        using var openDir = new OpenDialog(prompt, "Directory") { AllowsMultipleSelection = false };

        Application.Run(openDir, ConsoleMainWindow.ExceptionPopup);

        return openDir.FilePaths.Count == 1 ? new FileInfo(openDir.FilePaths[0]) : null;
    }

    public override FileInfo SelectFile(string prompt, string patternDescription, string pattern)
    {
        using var openDir = new OpenDialog(prompt, "File")
        {
            AllowsMultipleSelection = false,
            AllowedFileTypes = pattern == null ? null : new[] { pattern.TrimStart('*') }
        };

        Application.Run(openDir, ConsoleMainWindow.ExceptionPopup);

        var selected = openDir.FilePaths.Count == 1 ? openDir.FilePaths[0] : null;


        // entering "null" in a file dialog may return something like "D:\Blah\null"
        return string.Equals(Path.GetFileName(selected), "null", StringComparison.CurrentCultureIgnoreCase)
            ? null
            : selected == null
                ? null
                : new FileInfo(selected);
    }

    public override FileInfo[] SelectFiles(string prompt, string patternDescription, string pattern)
    {
        var openDir = new OpenDialog(prompt, "Directory")
        {
            AllowsMultipleSelection = true,
            AllowedFileTypes = pattern == null ? null : new[] { pattern.TrimStart('*') }
        };

        Application.Run(openDir, ConsoleMainWindow.ExceptionPopup);

        return openDir.FilePaths?.Select(f => new FileInfo(f))?.ToArray();
    }

    public override bool SelectEnum(DialogArgs args, Type enumType, out Enum chosen)
    {
        var dlg = new ConsoleGuiBigListBox<Enum>(args.WindowTitle, "Ok", false,
            Enum.GetValues(enumType).Cast<Enum>().ToList(), null, false);

        if (dlg.ShowDialog())
        {
            chosen = dlg.Selected;
            return true;
        }

        chosen = null;
        return false;
    }

    public override bool SelectType(DialogArgs args, Type[] available, out Type chosen)
    {
        var dlg = new ConsoleGuiBigListBox<Type>(args.WindowTitle, "Ok", true, available.ToList(), null, true);

        if (dlg.ShowDialog())
        {
            chosen = dlg.Selected;
            return true;
        }

        chosen = null;
        return false;
    }

    public override void ShowException(string errorText, Exception exception)
    {
        var msg = GetExceptionText(errorText, exception, false);

        var textView = new TextView
        {
            Text = msg,
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill() - 2,
            ReadOnly = true,
            AllowsTab = false,
            WordWrap = true
        };

        var toggleStack = true;

        var btnOk = new Button("Ok", true);
        btnOk.Clicked += () => Application.RequestStop();
        var btnStack = new Button("Stack");
        btnStack.Clicked += () =>
        {
            //flip between stack / no stack
            textView.Text = GetExceptionText(errorText, exception, toggleStack);
            textView.SetNeedsDisplay();
            toggleStack = !toggleStack;
        };

        GetDialogDimensions(out var w, out var h);

        var dlg = new Dialog("Error", w, h, btnOk, btnStack);
        dlg.Add(textView);

        Application.MainLoop.Invoke(() =>
            Application.Run(dlg, ConsoleMainWindow.ExceptionPopup)
        );
    }

    private static ustring GetExceptionText(string errorText, Exception exception, bool includeStackTrace) =>
        Wrap($"{errorText}\n{ExceptionHelper.ExceptionToListOfInnerMessages(exception, includeStackTrace)}", 76);

    private static string Wrap(string longString, int width)
    {
        return string.Join("\n", Regex.Matches(longString, $".{{1,{width}}}").Select(m => m.Value).ToArray());
    }

    public override void ShowData(IViewSQLAndResultsCollection collection)
    {
        var view = new ConsoleGuiSqlEditor(this, collection) { Modal = true };
        Application.Run(view, ConsoleMainWindow.ExceptionPopup);
    }

    public override void ShowData(DataTable table)
    {
        var view = new ConsoleGuiDataTableViewerUI(table) { Modal = true };
        Application.Run(view, ConsoleMainWindow.ExceptionPopup);
    }

    public override bool CanActivate(object o) => o is IMapsDirectlyToDatabaseTable || o is IMasqueradeAs;

    protected override void ActivateImpl(object o)
    {
        var m = o as IRevertable;

        if (o is IMasqueradeAs masq)
            if (masq.MasqueradingAs() is IRevertable underlyingObject)
                m = underlyingObject;

        if (m is CohortIdentificationConfiguration cic)
        {
            var view = new ConsoleGuiCohortIdentificationConfigurationUI(this, cic);
            Application.Run(view, ConsoleMainWindow.ExceptionPopup);
        }
        else if (m != null)
        {
            var view = new ConsoleGuiEdit(this, m) { Modal = true };
            Application.Run(view, ConsoleMainWindow.ExceptionPopup);
        }
    }

    public override void ShowLogs(ILoggedActivityRootObject rootObject)
    {
        var view = new ConsoleGuiViewLogs(this, rootObject);
        Application.Run(view, ConsoleMainWindow.ExceptionPopup);
    }

    public override void ShowGraph(AggregateConfiguration aggregate)
    {
        var view = new ConsoleGuiViewGraph(this, aggregate);
        Application.Run(view, ConsoleMainWindow.ExceptionPopup);
    }


    public override IPipelineRunner GetPipelineRunner(DialogArgs args, IPipelineUseCase useCase, IPipeline pipeline) =>
        new ConsoleGuiRunPipeline(this, useCase, pipeline);

    public override void LaunchSubprocess(ProcessStartInfo startInfo)
    {
        throw new NotSupportedException();
    }

    public override void ShowWarning(string message)
    {
        Show("Message", message);
    }
}