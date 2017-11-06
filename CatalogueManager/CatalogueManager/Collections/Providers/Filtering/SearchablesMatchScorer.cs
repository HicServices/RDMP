using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CatalogueLibrary.Providers;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;

namespace CatalogueManager.Collections.Providers.Filtering
{
    public class SearchablesMatchScorer
    {
        private static readonly int[] Weights = new int[] { 64, 32, 16, 8, 4, 2, 1 };


        public Dictionary<KeyValuePair<IMapsDirectlyToDatabaseTable, DescendancyList>, int> ScoreMatches(Dictionary<IMapsDirectlyToDatabaseTable, DescendancyList> searchables, string searchText, CancellationToken cancellationToken)
        {
            var tokens = searchText.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            List<int> integerTokens = new List<int>();

            foreach (string token in tokens)
            {
                int i;
                if (int.TryParse(token, out i))
                    integerTokens.Add(i);
            }

            var regexes = new List<Regex>();

            foreach (string token in tokens)
                regexes.Add(new Regex(Regex.Escape(token), RegexOptions.IgnoreCase));

            if (cancellationToken.IsCancellationRequested)
                return null;

            return searchables.ToDictionary(
           s => s,
           score => ScoreMatches(score, integerTokens, regexes, cancellationToken)
           );
        }

        private int ScoreMatches(KeyValuePair<IMapsDirectlyToDatabaseTable, DescendancyList> kvp, List<int> integerTokens, List<Regex> regexes, CancellationToken cancellationToken)
        {
            int score = 0;

            if (cancellationToken.IsCancellationRequested)
                return 0;

            //don't suggest AND/OR containers it's not helpful to navigate to these
            if (kvp.Key is CatalogueLibrary.Data.IContainer)
                return 0;

            //make a new list so we can destructively read it
            regexes = new List<Regex>(regexes);

            //match on ID of the head only
            foreach (int integerToken in integerTokens)
                if (kvp.Key.ID == integerToken)
                {
                    //matched on the ID (we could also match this in the tostring e.g. "project 132 my fishing project" where 132 is a number that is meaningful to the user only
                    var regex = regexes.SingleOrDefault(r => r.ToString().Equals(integerToken.ToString()));
                    if (regex != null)
                        regexes.Remove(regex);

                    score += Weights[0];
                }

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

            return score;
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
