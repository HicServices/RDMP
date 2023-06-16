// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Rdmp.Core.ReusableLibraryCode;

namespace Rdmp.UI.SimpleDialogs;

/// <summary>
/// Displays an in-depth technical report about an error that occurred during a task.  The error messages and the location in each class file in the stack is displayed including line 
/// numbers if a .pdb is available.  The pdb files for all RDMP code is shipped with the RDMP installer.
/// 
/// <para>Additionally as part of the build process of RDMP applications the built codebase is zipped into a file called SourceCodeForSelfAwareness.zip clicking one of the hyperlinks in
/// the stack trace will launch a small popup viewer showing you the codebase at that point with a highlight of the line that threw the Exception.</para>
/// 
/// </summary>
public partial class ExceptionViewerStackTraceWithHyperlinks : Form
{
    private static readonly Regex SourceFilePath = new(@" in (.*):line ");
    private static readonly Regex SourceCodeAvailable = new(@"\.cs:line (\d+)");
    private static readonly Regex MethodName = new(@"\.([A-Za-z_0-9][^\.]*)\(");
    private string _s;

    private ExceptionViewerStackTraceWithHyperlinks()
    {
        InitializeComponent();
    }
    public ExceptionViewerStackTraceWithHyperlinks(string environmentDotStackTrace):this()
    {
        AddTextToForm(environmentDotStackTrace);
    }

    public ExceptionViewerStackTraceWithHyperlinks(Exception exception): this()
    {
        AddTextToForm(ExceptionHelper.ExceptionToListOfInnerMessages(exception, true));
    }

    public ExceptionViewerStackTraceWithHyperlinks(StackTrace stackTrace): this()
    {
        AddTextToForm(stackTrace.ToString());
    }

    public static void MatchStackLine(string line, out Match filenameMatch, out Match lineNumberMatch)
    {
        lineNumberMatch = SourceCodeAvailable.Match(line);
        filenameMatch = SourceFilePath.Match(line);
    }

    public static bool MatchStackLine(string line, out string filenameMatch, out int lineNumberMatch,out string methodMatch)
    {

        var mline = SourceCodeAvailable.Match(line);
        var mfilename = SourceFilePath.Match(line);
        var m3 = MethodName.Match(line);

        if (mline.Success && mfilename.Success && m3.Success)
        {
            filenameMatch = mfilename.Groups[1].Value;
            lineNumberMatch = int.Parse(mline.Groups[1].Value);
            methodMatch = m3.Groups[1].Value;

            return true;
        }

        filenameMatch = null;
        lineNumberMatch = -1;
        methodMatch = null;

        return false;
    }

    private void AddTextToForm(string s)
    {
        _s = s;

        if (s == null)
            return;

        var lines = s.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        tableLayoutPanel1.RowCount = lines.Length;

        var tt = new ViewSourceCodeToolTip();

        for (var i = 0; i < lines.Length; i++)
        {
                
             
            //Any other things you want to not be a hyperlink because they give no useful context to the error can be added here and they will not appear as hyperlinks
            var lineIsMessageConstructor = lines[i].Contains("ReusableLibraryCode.Checks.CheckEventArgs..ctor");

            Match lineNumberMatch;
            Match filenameMatch;
            MatchStackLine(lines[i],out filenameMatch,out lineNumberMatch);

            if (!(lineNumberMatch.Success || filenameMatch.Success) || lineIsMessageConstructor)
            {
                var l = new Label();
                l.Text = lines[i];
                l.AutoSize = true;

                //it is a message not a stack trace line (stack trace lines start with <whitespace>at X
                if (!Regex.IsMatch(lines[i],@"^\s*at "))
                {
                    l.ForeColor = Color.Red;
                    l.Font = new Font(FontFamily.GenericMonospace, l.Font.Size);
                }

                tableLayoutPanel1.Controls.Add(l, 0, i);
            }
            else
            {
                var lineNumber = int.Parse(lineNumberMatch.Groups[1].Value);
                var link = new LinkLabel();

                var filename = filenameMatch.Groups[1].Value;

                link.AutoSize = true;
                link.Text = lines[i];

                tt.SetToolTip(link, $"{filename}|{lineNumber}");

                link.LinkClicked += (sender, args) => OpenVisualStudio(filename, lineNumber);
                tableLayoutPanel1.Controls.Add(link, 0, i);
            }
        }
    }


    internal static void Show(Exception _exception)
    {
        var dialog
            = new ExceptionViewerStackTraceWithHyperlinks(_exception);
        dialog.ShowDialog();
    }

    private void button2_Click(object sender, EventArgs e)
    {
        Clipboard.SetText(_s);
    }

    public static bool IsSourceCodeAvailable(Exception exception)
    {
        return exception != null  //exception exists
               &&
               !string.IsNullOrWhiteSpace(exception.StackTrace)  //and has a stack trace
               &&
               SourceCodeAvailable.IsMatch(exception.StackTrace); //and stack trace contains line numbers
    }


    private void OpenVisualStudio(string filename, int lineNumber)
    {
        try
        {
            Clipboard.SetText($"{Path.GetFileName(filename)}:{lineNumber}");

            var viewer = new ViewSourceCodeDialog(filename, lineNumber,Color.LawnGreen);
            viewer.ShowDialog();
        }
        catch (FileNotFoundException)
        {
            //there is no source code in the zip file
        }
        catch (Exception ex)
        {
            ExceptionViewer.Show(ex);
        }
            
    }
        
    private void button1_Click(object sender, EventArgs e)
    {
        Close();
    }

    private void ExceptionViewerStackTraceWithHyperlinks_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape || (e.KeyCode == Keys.W && e.Control))
            Close();
    }
}