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
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Providers.Nodes.PipelineNodes;
using ReusableLibraryCode;
using ReusableLibraryCode.Settings;

namespace Rdmp.Core.Providers;

/// <summary>
/// Scores objects as to how relevant they are to a given search string 
/// </summary>
public class SearchablesMatchScorer
{
    private static readonly int[] Weights = new int[] { 64, 32, 16, 8, 4, 2, 1 };

    public HashSet<string> TypeNames { get; set; }

    private readonly bool _scoreZeroForCohortAggregateContainers;
    private bool _showInternalCatalogues = true;
    private bool _showDeprecatedCatalogues = true;
    private bool _showColdStorageCatalogues = true;
    private bool _showProjectSpecificCatalogues = true;
    private bool _showNonExtractableCatalogues = true;


    /// <summary>
    /// List of objects which should be favoured slightly above others of equal match potential
    /// </summary>
    public List<IMapsDirectlyToDatabaseTable> BumpMatches { get; set; } = new List<IMapsDirectlyToDatabaseTable>();

    /// <summary>
    /// Only show objects with the given ID
    /// </summary>
    public int? ID { get; set; }

    /// <summary>
    /// How much to bump matches when they are in <see cref="BumpMatches"/>
    /// </summary>
    public int BumpWeight = 1;

    /// <summary>
    /// True to respect <see cref="UserSettings.ShowProjectSpecificCatalogues"/> etc settings.  Defaults to false
    /// </summary>
    public bool RespectUserSettings { get; set; } = false;


    /// <summary>
    /// Determines behaviour when there are no search terms.  If true then return an empty dictionary.
    /// If false return a dictionary in which all items are scored 0.
    /// </summary>
    public bool ReturnEmptyResultWhenNoSearchTerms { get; set; }

    /// <summary>
    /// When the user types one of these they get a filter on the full Type
    /// </summary>
    public static Dictionary<string, Type> ShortCodes =
        new Dictionary<string, Type> (StringComparer.CurrentCultureIgnoreCase){

            {"c",typeof (Catalogue)},
            {"ci",typeof (CatalogueItem)},
            {"sd",typeof (SupportingDocument)},
            {"p",typeof (Project)},
            {"ec",typeof (ExtractionConfiguration)},
            {"co",typeof (ExtractableCohort)},
            {"cic",typeof (CohortIdentificationConfiguration)},
            {"t",typeof (TableInfo)},
            {"col",typeof (ColumnInfo)},
            {"lmd",typeof (LoadMetadata)},
            {"pipe",typeof(Pipeline)},
            {"sds",typeof(SelectedDataSets)},
            {"eds",typeof(ExternalDatabaseServer)}

        };

    /// <summary>
    /// When the user types one of these Types (or a <see cref="ShortCodes"/> for one) they also get the value list for free.
    /// This lets you serve up multiple object Types e.g. <see cref="IMasqueradeAs"/> objects as though they were the same as thier
    /// Key Type.
    /// </summary>
    public static Dictionary<string, Type[]> AlsoIncludes =
        new Dictionary<string, Type[]> (StringComparer.CurrentCultureIgnoreCase){

            {"Pipeline",new Type[]{ typeof(PipelineCompatibleWithUseCaseNode)}}

        };

    public SearchablesMatchScorer()
    {
        TypeNames = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);

        _scoreZeroForCohortAggregateContainers = UserSettings.ScoreZeroForCohortAggregateContainers;
                
    }

    /// <summary>
    /// Performs a free text search on all <paramref name="searchables"/>.  The <paramref name="searchText"/> will match on both the object
    /// and its parental hierarchy e.g. "chi" "biochemistry" matches column "chi" in Catalogue "biochemistry" strongly.
    /// </summary>
    /// <param name="searchables">All available objects that can be searched (see <see cref="ICoreChildProvider.GetAllSearchables"/>)</param>
    /// <param name="searchText">Tokens to use separated by space e.g. "chi biochemistry CatalogueItem"</param>
    /// <param name="cancellationToken">Token for cancelling match scoring.  This method will return null if cancellation is detected</param>
    /// <param name="showOnlyTypes">Optional (can be null) list of types to return results from.  Not respected if <paramref name="searchText"/> includes type names</param>
    /// <returns></returns>
    public Dictionary<KeyValuePair<IMapsDirectlyToDatabaseTable, DescendancyList>, int> ScoreMatches(Dictionary<IMapsDirectlyToDatabaseTable, DescendancyList> searchables, string searchText, CancellationToken cancellationToken, List<Type> showOnlyTypes)
    {
        SetupRespectUserSettings();

        //do short code substitutions e.g. ti for TableInfo
        if (!string.IsNullOrWhiteSpace(searchText))
            foreach(var kvp in ShortCodes)
                searchText = Regex.Replace(searchText,$@"\b{kvp.Key}\b",kvp.Value.Name);
            
        //if user hasn't typed any explicit Type filters
        if(showOnlyTypes != null && TypeNames != null)
            //add the explicit types only if the search text does not contain any explicit type names
            if(string.IsNullOrWhiteSpace(searchText) || !TypeNames.Intersect(searchText.Split(' '),StringComparer.CurrentCultureIgnoreCase).Any())
                foreach (var showOnlyType in showOnlyTypes) 
                    searchText = searchText + " " + showOnlyType.Name;

        //Search the tokens for also inclusions e.g. "Pipeline" becomes "Pipeline PipelineCompatibleWithUseCaseNode"
        if (!string.IsNullOrWhiteSpace(searchText))
            foreach(var s in searchText.Split(' ').ToArray())
                if (AlsoIncludes.ContainsKey(s))
                    foreach(var v in AlsoIncludes[s])
                        searchText += " " + v.Name;

        //if we have nothing to search for return no results
        if (ReturnEmptyResultWhenNoSearchTerms && string.IsNullOrWhiteSpace(searchText) && ID == null)
            return new Dictionary<KeyValuePair<IMapsDirectlyToDatabaseTable, DescendancyList>, int>();

        var tokens = (searchText??"").Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            
        var regexes = new List<Regex>();
            
        //any token that 100% matches a type name is an explicitly typed token
        string[] explicitTypesRequested;

        if (TypeNames != null)
        {
            explicitTypesRequested = TypeNames.Intersect(tokens,StringComparer.CurrentCultureIgnoreCase).ToArray();

            //else it's a regex
            foreach (string token in tokens.Except(TypeNames,StringComparer.CurrentCultureIgnoreCase))
                regexes.Add(new Regex(Regex.Escape(token), RegexOptions.IgnoreCase));

        }
        else
            explicitTypesRequested = new string[0];

        if (cancellationToken.IsCancellationRequested)
            return null;
            
        return searchables.ToDictionary(
            s => s,
            score => ScoreMatches(score, regexes,explicitTypesRequested, cancellationToken)
        );
    }

    private void SetupRespectUserSettings()
    {
        _showInternalCatalogues = RespectUserSettings ? UserSettings.ShowInternalCatalogues : true;
        _showDeprecatedCatalogues = RespectUserSettings ? UserSettings.ShowDeprecatedCatalogues : true;
        _showColdStorageCatalogues = RespectUserSettings ? UserSettings.ShowColdStorageCatalogues : true;
        _showProjectSpecificCatalogues = RespectUserSettings ? UserSettings.ShowProjectSpecificCatalogues : true;
        _showNonExtractableCatalogues = RespectUserSettings ? UserSettings.ShowNonExtractableCatalogues : true;
    }

    private int ScoreMatches(KeyValuePair<IMapsDirectlyToDatabaseTable, DescendancyList> kvp, List<Regex> regexes, string[] explicitTypeNames, CancellationToken cancellationToken)
    {
        int score = 0;

        if (cancellationToken.IsCancellationRequested)
            return 0;

        //if we are searching for a specific ID
        if (ID.HasValue)
        {
            var obj = kvp.Key;

            //If the object is masquerading as something else, better to check that
            if (kvp.Key is IMasqueradeAs m)
            {
                obj = m.MasqueradingAs() as IMapsDirectlyToDatabaseTable ?? obj;
            }

            if (obj.ID != ID.Value)
                return 0;
            else
                score += 10;
        }

        if(RespectUserSettings && ScoreZeroBecauseOfUserSettings(kvp))
        {
            return 0;
        }

        // if user is searching for a specific Type of object and we ain't it
        if (explicitTypeNames.Any())
            if (!explicitTypeNames.Contains(kvp.Key.GetType().Name))
                return 0;

        //don't suggest AND/OR containers it's not helpful to navigate to these (unless user is searching by Type explicitly)
        if (explicitTypeNames.Length == 0 && kvp.Key is IContainer)
            return 0;

        //don't suggest AND/OR containers it's not helpful to navigate to these (unless user is searching by Type explicitly)
        if ( _scoreZeroForCohortAggregateContainers && explicitTypeNames.Length == 0 && kvp.Key is CohortAggregateContainer)
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

        //match on the parents if there's a decendancy list
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
                    if (!(parent is IContainer))
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
            
        //if we are bumping up matches
        if (score > 0 && BumpMatches.Contains(kvp.Key)) 
            score += BumpWeight;

        return score;
    }

    /// <summary>
    /// Returns true if the given <paramref name="kvp"/> object isnot one the user wants to ever see based on the values e.g. <see cref="UserSettings.ShowDeprecatedCatalogues"/>
    /// </summary>
    /// <param name="kvp"></param>
    /// <returns></returns>
    private bool ScoreZeroBecauseOfUserSettings(KeyValuePair<IMapsDirectlyToDatabaseTable, DescendancyList> kvp)
    {
        return !Filter(kvp.Key, kvp.Value, _showInternalCatalogues, _showDeprecatedCatalogues, _showColdStorageCatalogues, _showProjectSpecificCatalogues, _showNonExtractableCatalogues);
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

        //exact matches
        foreach (var match in regexes.Where(r => r.ToString().Equals(str,StringComparison.CurrentCultureIgnoreCase)).ToArray())
        {
            regexes.Remove(match);
            //score double for exact matches
            matches+=2;
        }
            
        //contains matches
        foreach (var match in regexes.Where(r => r.IsMatch(str)).ToArray())
        {
            regexes.Remove(match);
            matches++;
        }

        return matches;
    }

    /// <summary>
    /// Returns true if the given <paramref name="modelObject"/> survives filtering based on the supplied inclusion
    /// criteria.  Anything that isn't in some way related to a <see cref="Catalogue"/> automatically survives filtering
    /// </summary>
    /// <param name="modelObject"></param>
    /// <param name="descendancy"></param>
    /// <param name="includeInternal"></param>
    /// <param name="includeDeprecated"></param>
    /// <param name="includeColdStorage"></param>
    /// <param name="includeProjectSpecific"></param>
    /// <param name="includeNonExtractable"></param>
    /// <returns>True if the item should be shown to the user based on filters</returns>
    public static bool Filter(object modelObject, DescendancyList descendancy, bool includeInternal, bool includeDeprecated, bool includeColdStorage, bool includeProjectSpecific, bool includeNonExtractable)
    {
        var cata = modelObject as ICatalogue;

        //doesn't relate to us... 
        if (cata == null)
        {
            // or are we one of these things that can be tied to a catalogue
            cata = modelObject switch
            {
                ExtractableDataSet eds => eds.Catalogue,
                SelectedDataSets sds => sds.GetCatalogue(),
                _ => descendancy?.Parents.OfType<Catalogue>().SingleOrDefault()
            };

            if (cata == null)
                return true;
        }

        bool isProjectSpecific = cata.IsProjectSpecific(null);
        bool isExtractable = cata.GetExtractabilityStatus(null) != null && cata.GetExtractabilityStatus(null).IsExtractable;

        return (isExtractable && !cata.IsColdStorageDataset && !cata.IsDeprecated && !cata.IsInternalDataset && !isProjectSpecific) ||
               ((includeColdStorage && cata.IsColdStorageDataset) ||
                (includeDeprecated && cata.IsDeprecated) ||
                (includeInternal && cata.IsInternalDataset) ||
                (includeProjectSpecific && isProjectSpecific) ||
                (includeNonExtractable && !isExtractable));

    }

    /// <summary>
    /// Shortlists the output of <see cref="ScoreMatches(Dictionary{IMapsDirectlyToDatabaseTable, DescendancyList}, string, CancellationToken, List{Type})"/>
    /// producing a list of results up to the supplied length (<paramref name="take"/>).
    /// </summary>
    /// <param name="scores"></param>
    /// <param name="take"></param>
    /// <param name="activator"></param>
    /// <returns></returns>
    public List<IMapsDirectlyToDatabaseTable> ShortList(Dictionary<KeyValuePair<IMapsDirectlyToDatabaseTable, DescendancyList>, int> scores, int take, IBasicActivateItems activator)
    {
        var favourites = activator.FavouritesProvider.CurrentFavourites;

        return scores.Where(score => score.Value > 0)
            .OrderByDescending(score => score.Value)
            .ThenByDescending(kvp => favourites.Any(f=>f.IsReferenceTo(kvp.Key.Key))) // favour favourites
            .ThenBy(kvp => kvp.Key.Key.ToString()) // sort ties alphabetically
            .Take(take)
            .Select(score => score.Key.Key)
            .ToList();
    }
}