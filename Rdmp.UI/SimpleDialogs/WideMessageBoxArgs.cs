// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.


namespace Rdmp.UI.SimpleDialogs;

/// <summary>
/// Initialization object for <see cref="WideMessageBox"/> (controls look and feel, content etc)
/// </summary>
public class WideMessageBoxArgs
{
    public string Title { get; set; }
    public string Message { get; set; }
    public string EnvironmentDotStackTrace { get; set; }
    public string KeywordNotToAdd { get; set; }
    public WideMessageBoxTheme Theme { get; set; }
    public bool FormatAsParagraphs { get; set; }

    public WideMessageBoxArgs(string title, string message, string environmentDotStackTrace, string keywordNotToAdd,
        WideMessageBoxTheme theme)
    {
        Title = title;
        Message = message;
        EnvironmentDotStackTrace = environmentDotStackTrace;
        KeywordNotToAdd = keywordNotToAdd;
        Theme = theme;
    }
}