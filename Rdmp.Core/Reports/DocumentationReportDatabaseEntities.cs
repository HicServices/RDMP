// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

//specially for MSword file

using System;
using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Spontaneous;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Comments;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;

namespace Rdmp.Core.Reports;

/// <summary>
///     Generates class descriptions for all <see cref="DatabaseEntity" /> classes (from xmldocs) into a user readable
///     Microsoft Word Docx file.
///     Also shows corresponding icon within RDMP.  This allows the user to see what a Project is and the icon what an
///     ExtractionConfiguration
///     is etc and for those descriptions/icons to always match the live/installed version of RDMP.
/// </summary>
public class DocumentationReportDatabaseEntities : DocXHelper
{
    private CommentStore _commentStore;
    private readonly Dictionary<Type, string> Summaries = new();

    public void GenerateReport(CommentStore commentStore, ICheckNotifier notifier, IIconProvider iconProvider,
        bool showFile)
    {
        _commentStore = commentStore;
        try
        {
            Check(notifier);

            using var document = GetNewDocFile("RDMPDocumentation");
            var t = InsertTable(document, Summaries.Count * 2 + 1, 1);

            //Listing Cell header
            SetTableCell(t, 0, 0, "Tables");

            var keys = Summaries.Keys.ToArray();

            for (var i = 0; i < Summaries.Count; i++)
            {
                //creates the run
                SetTableCell(t, i * 2 + 1, 0, "");

                var bmp = iconProvider.GetImage(keys[i]);

                if (bmp != null)
                {
                    var para = t.Rows[i * 2 + 1].GetCell(0).Paragraphs.First();
                    var run = para.Runs.FirstOrDefault() ?? para.CreateRun();
                    GetPicture(run, bmp);
                }

                SetTableCell(t, i * 2 + 1, 0, $" {keys[i].Name}");

                SetTableCell(t, i * 2 + 2, 0, Summaries[keys[i]]);
            }

            if (showFile)
                ShowFile(document);
        }
        catch (Exception e)
        {
            notifier.OnCheckPerformed(new CheckEventArgs("Report generation failed", CheckResult.Fail, e));
        }
    }

    private void Check(ICheckNotifier notifier)
    {
        foreach (var t in MEF.GetAllTypes().Where(t => typeof(DatabaseEntity).IsAssignableFrom(t)))
            if (typeof(IMapsDirectlyToDatabaseTable).IsAssignableFrom(t))
            {
                if (t.IsInterface || t.IsAbstract || t.Name.StartsWith("Spontaneous") ||
                    t.Assembly.FullName?.StartsWith("DynamicProxyGenAssembly2") == true)
                    continue;
                try
                {
                    //spontaneous objects don't exist in the database.
                    if (typeof(SpontaneousObject).IsAssignableFrom(t))
                        continue;
                }
                catch (Exception)
                {
                    continue;
                }

                notifier.OnCheckPerformed(new CheckEventArgs($"Found type {t}", CheckResult.Success));

                var docs = _commentStore.GetTypeDocumentationIfExists(t, true, true);

                if (docs == null)
                    notifier.OnCheckPerformed(
                        new CheckEventArgs($"Failed to get definition for class {t.FullName} defined in {t.Assembly}",
                            CheckResult.Fail));
                else
                    Summaries.Add(t, docs);
            }
    }
}