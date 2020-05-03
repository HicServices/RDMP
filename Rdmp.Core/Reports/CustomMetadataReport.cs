// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataQualityEngine;
using Rdmp.Core.Repositories;

namespace Rdmp.Core.Reports
{

    /// <summary>
    /// Create a custom report e.g. markdown, xml etc by taking a template file and replicating it with replacements for each <see cref="Catalogue"/> property
    /// </summary>
    public class CustomMetadataReport
    {

        Dictionary<string,Func<Catalogue,object>> Replacements = new Dictionary<string, Func<Catalogue,object>>();

        private readonly IDetermineDatasetTimespan _timespanCalculator = new DatasetTimespanCalculator();

        public CustomMetadataReport()
        {
            //add basic properties
            foreach (var prop in typeof(Catalogue).GetProperties())
                Replacements.Add("$" + prop.Name, (s) => prop.GetValue(s));

            Replacements.Add("$StartDate",
                (c) => _timespanCalculator?.GetMachineReadableTimepsanIfKnownOf(c, true, out _)?.Item1?.ToString());
            Replacements.Add("$EndDate",
                (c) => _timespanCalculator?.GetMachineReadableTimepsanIfKnownOf(c, true, out _)?.Item2?.ToString());
            Replacements.Add("$DateRange",
                (c) => _timespanCalculator?.GetHumanReadableTimepsanIfKnownOf(c, true, out _));
        }
        
        public void GenerateReport(Catalogue[] catalogues, DirectoryInfo outputDirectory, FileInfo template, string fileNaming, bool oneFile)
        {
            if(catalogues == null || !catalogues.Any())
                return;
            
            var templateBody = File.ReadAllText(template.FullName);

            string outname = DoReplacements(fileNaming,catalogues.First());

            StreamWriter outFile = null;
            
            if(oneFile)
                outFile = new StreamWriter(File.Create(Path.Combine(outputDirectory.FullName, outname)));

            foreach (Catalogue catalogue in catalogues)
            {
                var newContents = DoReplacements(templateBody, catalogue);

                if (oneFile) 
                    outFile.WriteLine(newContents);
                else
                {
                    using (var sw = new StreamWriter(Path.Combine(outputDirectory.FullName,
                        DoReplacements(fileNaming, catalogue))))
                    {
                        sw.Write(newContents);
                        sw.Flush();
                        sw.Close();
                    }
                }
            }
            outFile?.Flush();
            outFile?.Dispose();
        }

        private string DoReplacements(string str, Catalogue catalogue)
        {
            foreach (var r in Replacements) 
                str = str.Replace(r.Key, r.Value(catalogue)?.ToString() ?? "");

            return str;
        }
    }
}
