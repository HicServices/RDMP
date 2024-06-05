// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;
using Rdmp.UI.ScintillaHelper;
using ScintillaNET;

namespace Rdmp.UI.SimpleDialogs;

/// <summary>
/// Allows you to view a class file from the RDMP codebase.  See ExceptionViewerStackTraceWithHyperlinks for the mechanics of how this works (or UserManual.md).  A green line will
/// highlight the line on which the message or error occurred.
/// </summary>
[TechnicalUI]
public partial class ViewSourceCodeDialog : Form
{
    private readonly Scintilla _queryEditor;

    private static readonly HashSet<FileInfo> SupplementalSourceZipFiles = new();
    private static readonly object oSupplementalSourceZipFilesLock = new();
    private const string MainSourceCodeRepo = "SourceCodeForSelfAwareness.zip";

    public static void AddSupplementalSourceZipFile(FileInfo f)
    {
        lock (oSupplementalSourceZipFilesLock)
        {
            SupplementalSourceZipFiles.Add(f);
        }
    }

    public ViewSourceCodeDialog(string filename, int lineNumber, Color highlightColor)
    {
        var toFind = Path.GetFileName(filename);

        InitializeComponent();

        if (filename == null)
            return;

        var designMode = LicenseManager.UsageMode == LicenseUsageMode.Designtime;

        if (designMode) //don't add the QueryEditor if we are in design time (visual studio) because it breaks
            return;

        _queryEditor = new ScintillaTextEditorFactory().Create(null, SyntaxLanguage.CSharp);

        panel1.Controls.Add(_queryEditor);

        LoadSourceCode(toFind, lineNumber, highlightColor);

        var worker = new BackgroundWorker();
        worker.DoWork += WorkerOnDoWork;
        worker.RunWorkerAsync();
    }

    private void LoadSourceCode(string toFind, int lineNumber, Color highlightColor)
    {
        lock (oSupplementalSourceZipFilesLock)
        {
            var readToEnd = GetSourceForFile(toFind);

            //entry was found
            if (readToEnd != null)
            {
                _queryEditor.Text = readToEnd;

                if (lineNumber != -1)
                {
                    _queryEditor.FirstVisibleLine = Math.Max(0, lineNumber - 10);
                    ScintillaLineHighlightingHelper.HighlightLine(_queryEditor, lineNumber - 1, highlightColor);
                }
            }
            else
            {
                throw new FileNotFoundException($"Could not find file called '{toFind}' in any of the zip archives");
            }
        }

        Text = toFind;
    }

    private void WorkerOnDoWork(object sender, DoWorkEventArgs doWorkEventArgs)
    {
        var entries = new HashSet<string>();

        var zipArchive = new FileInfo(MainSourceCodeRepo);
        foreach (var zipFile in new[] { zipArchive }.Union(SupplementalSourceZipFiles).Where(static zipFile => zipFile.Exists))
        {
            using var z = ZipFile.OpenRead(zipFile.FullName);
            foreach (var entry in z.Entries)
                entries.Add(entry.Name);
        }


        olvSourceFiles.AddObjects(entries.ToArray());
    }

    public ViewSourceCodeDialog(string filename) : this(filename, -1, Color.White)
    {
    }

    public static string GetSourceForFile(string toFind)
    {
        try
        {
            var zipArchive = new FileInfo(MainSourceCodeRepo);

            //for each zip file (starting with the main archive)
            foreach (var zipFile in new[] { zipArchive }.Union(SupplementalSourceZipFiles))
                //if the zip exists
                if (zipFile.Exists)
                {
                    //read the entry (if it is there)
                    using var z = ZipFile.OpenRead(zipFile.FullName);
                    var readToEnd = GetEntryFromZipFile(z, toFind);

                    if (readToEnd != null) //the entry was found and read
                        return readToEnd;
                }
        }
        catch (Exception)
        {
            return null;
        }

        //couldn't find any text
        return null;
    }

    public static bool SourceCodeIsAvailableFor(string s)
    {
        try
        {
            return GetSourceForFile(s) != null;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static string GetEntryFromZipFile(ZipArchive z, string toFind)
    {
        var entry = z.Entries.FirstOrDefault(e => e.Name == toFind);

        return entry == null ? null : new StreamReader(entry.Open()).ReadToEnd();
    }

    private void textBox1_TextChanged(object sender, EventArgs e)
    {
        olvSourceFiles.UseFiltering = true;
        olvSourceFiles.ModelFilter =
            new TextMatchFilter(olvSourceFiles, textBox1.Text, StringComparison.CurrentCultureIgnoreCase);
    }

    private void olvSourceFiles_ItemActivate(object sender, EventArgs e)
    {
        if (olvSourceFiles.SelectedObject is string str)
            LoadSourceCode(str, -1, Color.White);
    }
}