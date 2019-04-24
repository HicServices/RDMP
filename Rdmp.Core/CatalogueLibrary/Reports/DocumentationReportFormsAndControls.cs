// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Comments;

namespace Rdmp.Core.CatalogueLibrary.Reports
{
    /// <summary>
    /// Extracts comments from SourceCodeForSelfAwareness.zip for the requested Types which are made available in the public property Summaries.  This ensures that you always
    /// have the latest documentation available at runtime and that descriptions match the codebase 100%.
    /// </summary>
    public class DocumentationReportFormsAndControls: ICheckable
    {
        private readonly CommentStore _commentStore;
        private readonly Type[] _formsAndControls;
        public Dictionary<Type, string> Summaries = new Dictionary<Type, string>();

        public DocumentationReportFormsAndControls(CommentStore commentStore,Type[] formsAndControls)
        {
            _commentStore = commentStore;
            _formsAndControls = formsAndControls;
        }

        public void Check(ICheckNotifier notifier)
        {
            foreach (Type t in _formsAndControls)
            {
                if(t.Name.EndsWith("_Design"))
                    continue;

                try
                {
                    //spontaneous objects don't exist in the database.
                    notifier.OnCheckPerformed(new CheckEventArgs("Found Type " + t.Name, CheckResult.Success,null));
                }
                catch(Exception)
                {
                    continue;
                }

                //it's an abstract empty design class
                if(t.Name.EndsWith("_Design"))
                    continue;

                var docs = _commentStore.GetTypeDocumentationIfExists(t);

                if(docs != null)
                {
                    if (!Summaries.ContainsKey(t))
                        Summaries.Add(t, docs);
                }
                else
                    notifier.OnCheckPerformed(
                        new CheckEventArgs("Failed to get definition for class " + t.FullName, CheckResult.Fail));

            }
            
        }

        public static string StripTags(string s)
        {
            string toReturn = s.Replace("<para>", "").Replace("</para>", "").Replace("<see cref=\"","").Replace("\"/>","");
            
            return toReturn;
        }
    }
}
