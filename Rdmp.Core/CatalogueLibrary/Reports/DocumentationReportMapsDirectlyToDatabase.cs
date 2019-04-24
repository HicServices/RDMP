// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Reflection;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Comments;

namespace CatalogueLibrary.Reports
{
    /// <summary>
    /// Extracts comments from SourceCodeForSelfAwareness.zip for all IMapsDirectlyToDatabaseTable object Types (all things that the user can store in the Catalogue/Data Export
    /// repository - e.g. Catalogues, TableInfos etc).  This ensures that the help is always up-to-date and matches the source code 100%.
    /// </summary>
    public class DocumentationReportMapsDirectlyToDatabase: ICheckable
    {
        private readonly CommentStore _commentStore;
        private readonly Assembly[] _assemblies;

        public Dictionary<Type, string> Summaries = new Dictionary<Type, string>();
        
        public DocumentationReportMapsDirectlyToDatabase(CommentStore commentStore, params Assembly[] assemblieses)
        {
            _commentStore = commentStore;
            _assemblies = assemblieses;
        }

        public void Check(ICheckNotifier notifier)
        {
            foreach (Assembly assembly in _assemblies)
                foreach (Type t in assembly.GetTypes())
                    if (typeof (IMapsDirectlyToDatabaseTable).IsAssignableFrom(t))
                    {
                        if (t.IsInterface || t.IsAbstract)
                            continue;
                        try
                        {
                            //spontaneous objects don't exist in the database.
                            if(t.Name.StartsWith("Spontaneous"))
                                continue;
                        }
                        catch(Exception)
                        {
                            continue;
                        }

                        notifier.OnCheckPerformed(new CheckEventArgs("Found type " + t, CheckResult.Success));

                        if(_commentStore.ContainsKey(t.Name))
                            Summaries.Add(t, _commentStore[t.Name]);
                        else if(_commentStore.ContainsKey("I" + t.Name))
                            Summaries.Add(t, _commentStore["I" + t.Name]);
                        else
                            notifier.OnCheckPerformed(
                                new CheckEventArgs("Failed to get definition for class " + t.FullName, CheckResult.Fail));
                    }
        }
    }
}
