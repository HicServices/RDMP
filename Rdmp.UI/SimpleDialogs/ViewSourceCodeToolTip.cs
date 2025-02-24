// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;


namespace Rdmp.UI.SimpleDialogs;

[System.ComponentModel.DesignerCategory("")]
internal class ViewSourceCodeToolTip : ToolTip
{
    private int WIDTH = 600;
    private int HEIGHT = 450;
    private int LINE_PADDING = 1;

    public ViewSourceCodeToolTip()
    {
        OwnerDraw = true;
        ToolTipIcon = ToolTipIcon.None;
        ToolTipTitle = string.Empty;

        Popup += OnPopup;
        Draw += OnDraw;
    }


    private void OnPopup(object sender, PopupEventArgs e)
    {
        if (Screen.PrimaryScreen != null && Screen.PrimaryScreen.Bounds != Rectangle.Empty)
        {
            //use half the screen width or 600 if they are playing on a game boy advanced
            WIDTH = Math.Max(600, Screen.PrimaryScreen.Bounds.Width / 2);
            HEIGHT = Math.Max(450, Screen.PrimaryScreen.Bounds.Height / 2);
        }

        e.ToolTipSize = new Size(WIDTH, HEIGHT);
    }

    private static Dictionary<string, string[]> SourceFileCache = new();

    private void OnDraw(object sender, DrawToolTipEventArgs e)
    {
        try
        {
            var elements = e.ToolTipText.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
            var filename = elements[0];
            var linenumber = int.Parse(elements[1]);

            linenumber = Math.Max(0, linenumber - 1);

            var lines = ReadAllLinesCached(filename) ??
                        throw new FileNotFoundException(
                            $"Could not find source code for file:{Path.GetFileName(filename)}");

            //get height of any given line
            var coreLineHeight = e.Graphics.MeasureString("I've got a lovely bunch of coconuts", e.Font).Height +
                                 LINE_PADDING * 2f;

            var midpointY = HEIGHT / 2;

            //white background
            e.Graphics.FillRectangle(Brushes.White, 0, 0, WIDTH, HEIGHT);

            //the highlighted line
            e.Graphics.FillRectangle(Brushes.LawnGreen, 0, midpointY - LINE_PADDING, WIDTH, coreLineHeight);
            e.Graphics.DrawString(lines[linenumber], e.Font, Brushes.Black, 0, midpointY);

            var index = linenumber - 1;
            var currentLineY = midpointY - coreLineHeight;

            //any other lines we can fit on above the current line
            while (currentLineY > 0 && index >= 0)
            {
                e.Graphics.DrawString(lines[index], e.Font, Brushes.Black, 0, currentLineY);
                currentLineY -= coreLineHeight;
                index--;
            }

            index = linenumber + 1;
            currentLineY = midpointY + coreLineHeight;

            //while there are lines below us
            while (currentLineY < HEIGHT && index < lines.Length)
            {
                e.Graphics.DrawString(lines[index], e.Font, Brushes.Black, 0, currentLineY);
                currentLineY += coreLineHeight;
                index++;
            }

            //draw the name of the file
            e.Graphics.FillRectangle(Brushes.DarkBlue, 0, 0, WIDTH, coreLineHeight);
            e.Graphics.DrawString(Path.GetFileName(filename), e.Font, Brushes.White, LINE_PADDING, LINE_PADDING);
        }
        catch (Exception exception)
        {
            //white background
            e.Graphics.FillRectangle(Brushes.White, 0, 0, WIDTH, HEIGHT);
            e.Graphics.DrawString(exception.Message, e.Font, Brushes.Red, new RectangleF(0, 0, WIDTH, HEIGHT));
        }
    }

    private static string[] ReadAllLinesCached(string filename)
    {
        if (SourceFileCache.TryGetValue(filename, out var fileContents)) return fileContents;

        //if you have the original file
        if (File.Exists(filename))
        {
            fileContents = File.ReadLines(filename).ToArray();
        }
        //otherwise get it from SourceCodeForSelfAwareness.zip / Plugin zip source codes
        else
        {
            var contentsInOneLine = ViewSourceCodeDialog.GetSourceForFile(Path.GetFileName(filename));

            if (contentsInOneLine == null)
                return null;

            fileContents = contentsInOneLine.Split('\n');
        }

        SourceFileCache.Add(filename, fileContents);

        return fileContents;
    }
}