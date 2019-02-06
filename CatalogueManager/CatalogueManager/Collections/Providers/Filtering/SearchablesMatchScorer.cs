// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using CatalogueLibrary.Data;
using CatalogueLibrary.Providers;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;

namespace CatalogueManager.Collections.Providers.Filtering
{
    public class SearchablesMatchScorer
    {
        private static readonly int[] Weights = new int[] { 64, 32, 16, 8, 4, 2, 1 };

        public HashSet<string> TypeNames { get; set; }

        public SearchablesMatchScorer()
        {
            TypeNames = new HashSet<string>();
        }

        public Dictionary<KeyValuePair<IMapsDirectlyToDatabaseTable, DescendancyList>, int> ScoreMatches(Dictionary<IMapsDirectlyToDatabaseTable, DescendancyList> searchables, string searchText, CancellationToken cancellationToken)
        {
            var tokens = (searchText??"").Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            
            var regexes = new List<Regex>();
            
            //any token that 100% matches a type name is an explicitly typed token
            IEnumerable<string> explicitTypesRequested;

            if (TypeNames != null)
            {
                explicitTypesRequested = TypeNames.Intersect(tokens);

                //else it's a regex
                foreach (string token in tokens.Except(TypeNames))
                    regexes.Add(new Regex(Regex.Escape(token), RegexOptions.IgnoreCase));

            }
            else
                explicitTypesRequested = new string[0];

            if (cancellationToken.IsCancellationRequested)
                return null;

            return searchables.ToDictionary(
           s => s,
           score => ScoreMatches(score, regexes,explicitTypesRequested.ToArray(), cancellationToken)
           );
        }

        private int ScoreMatches(KeyValuePair<IMapsDirectlyToDatabaseTable, DescendancyList> kvp, List<Regex> regexes, string[] explicitTypeNames, CancellationToken cancellationToken)
        {
            int score = 0;

            if (cancellationToken.IsCancellationRequested)
                return 0;

            if (explicitTypeNames.Any())
                if (!explicitTypeNames.Contains(kvp.Key.GetType().Name))
                    return 0;

           //don't suggest AND/OR containers it's not helpful to navigate to these
            if (kvp.Key is CatalogueLibrary.Data.IContainer)
                return 0;

            //don't suggest AND/OR containers it's not helpful to navigate to these
            if (kvp.Key is CatalogueLibrary.Data.Cohort.CohortAggregateContainer)
                return 0;

            //if there are no tokens
            if (!regexes.Any())
                if (explicitTypeNames.Any()) //if they have so far just typed a TypeName
                    return 1;
                else
                    return 1;//no regexes AND no TypeName what did they type! whatever everyone scores the same
            
            //make a new list so we can destructively read it
            regexes = new List<Regex>(regexes);
            
            //match on the head vs the regex tokens
            score += Weights[0] * CountMatchToString(regexes, kvp.Key);

            score += Weights[0] * CountMatchType(regexes, kvp.Key);

            //match on the parents if theres a decendancy list
            if (kvp.Value != null)
            {
                var parents = kvp.Value.Parents;
                int numberOfParents = parents.Length;

                //for each prime after the first apply it as a multiple of the parent match
                for (int i = 1; i < Weights.Length; i++)
                {
                    //if we have run out of parents
                    if (i > numberOfParents)
                        break;

                    var parent = parents[parents.Length - i];

                    if (parent != null)
                    {
                        if (!(parent is CatalogueLibrary.Data.IContainer))
                        {
                            score += Weights[i] * CountMatchToString(regexes, parent);
                            score += Weights[i] * CountMatchType(regexes, parent);
                        }
                    }
                }
            }

            //if there were unmatched regexes
            if (regexes.Any())
                return 0;

            Catalogue catalogueIfAny = GetCatalogueIfAnyInDescendancy(kvp);

            if (catalogueIfAny != null && catalogueIfAny.IsDeprecated)
                return score /10;
            
            return score;
        }

        private Catalogue GetCatalogueIfAnyInDescendancy(KeyValuePair<IMapsDirectlyToDatabaseTable, DescendancyList> kvp)
        {
            if (kvp.Key is Catalogue)
                return (Catalogue) kvp.Key;

            if (kvp.Value != null)
                return (Catalogue)kvp.Value.Parents.FirstOrDefault(p => p is Catalogue);

            return null;
        }

        private int CountMatchType(List<Regex> regexes, object key)
        {
            return MatchCount(regexes, key.GetType().Name);
        }
        private int CountMatchToString(List<Regex> regexes, object key)
        {
            var s = key as ICustomSearchString;
            string matchOn = s != null ? s.GetSearchString() : key.ToString();

            return MatchCount(regexes, matchOn);
        }
        private int MatchCount(List<Regex> regexes, string str)
        {
            int matches = 0;
            foreach (var match in regexes.Where(r => r.IsMatch(str)).ToArray())
            {
                regexes.Remove(match);
                matches++;
            }

            return matches;
        }
    }
}
