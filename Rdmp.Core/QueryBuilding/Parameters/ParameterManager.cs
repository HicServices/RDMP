// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Spontaneous;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using IFilter = Rdmp.Core.Curation.Data.IFilter;

namespace Rdmp.Core.QueryBuilding.Parameters;

/// <summary>
/// Handles the accumulation of Parameters in SQL queries ('DECLARE @bob as varchar(10); SET @bob = 'bob').  Because ISqlParameters can exist at many levels
/// (e.g. IFilter vs AggregateConfiguration) ParameterManager has to consider what ParameterLevel it finds ISqlParameters at and whether ISqlParameters are
/// functionality identical or not.  For example if a CohortIdentificationConfiguration has two AggregateConfigurations each with an IFilter for healthboard
/// containing an ISqlParameter @hb.  If the declaration and value are the same then the ParameterManager can ignore one.  If the values are different then
/// the ParameterManager needs to create renamed versions in memory (SpontaneouslyInventedSqlParameter) and update the IFilter.  However if there is an ISqlParameter
/// @hb declared at the root (the CohortIdentificationConfiguration) then it will override both and be used instead.
/// 
/// <para>See ParameterLevel for a description of the various levels ISqlParameters can be found at.</para>
/// 
/// <para>ParameterManager has a ParameterManagerLifecycleState (State) which indicates whether it is still collecting new ISqlParameters or whether it has resolved them
/// into a final representation.  </para>
/// 
/// <para>You can merge multiple ParameterManagers together (like CohortQueryBuilder does) by calling ImportAndElevateResolvedParametersFromSubquery which will create the
/// final CompositeQueryLevel. </para>
/// </summary>
public class ParameterManager
{
    /// <summary>
    /// <see cref="ParameterManager"/> is a state driven object, it gathers all <see cref="ISqlParameter"/> then resolves them into a final list.  This
    /// property records which stage of that lifecycle it is at.
    /// </summary>
    public ParameterManagerLifecycleState State { get; private set; }

    /// <summary>
    /// Collection of all the parameters found at each level so far
    /// <para>Do not modify this yourself</para>
    /// </summary>
    public Dictionary<ParameterLevel, List<ISqlParameter>> ParametersFoundSoFarInQueryGeneration = new();

    /// <summary>
    /// Repository for creating temporary aggregate parameters
    /// </summary>
    private readonly MemoryRepository _memoryRepository = new();

    /// <summary>
    /// Creates a new <see cref="ParameterManager"/> with the specified global parameters
    /// </summary>
    /// <param name="globals"></param>
    public ParameterManager(ISqlParameter[] globals = null)
    {
        State = ParameterManagerLifecycleState.AllowingGlobals;

        ParametersFoundSoFarInQueryGeneration.Add(ParameterLevel.TableInfo, new List<ISqlParameter>());
        ParametersFoundSoFarInQueryGeneration.Add(ParameterLevel.QueryLevel, new List<ISqlParameter>());
        ParametersFoundSoFarInQueryGeneration.Add(ParameterLevel.CompositeQueryLevel, new List<ISqlParameter>());
        ParametersFoundSoFarInQueryGeneration.Add(ParameterLevel.Global, new List<ISqlParameter>());

        if (globals != null)
            ParametersFoundSoFarInQueryGeneration[ParameterLevel.Global].AddRange(globals);
    }

    /// <summary>
    /// Records parameters from the <see cref="TableInfo"/> at the appropriate <see cref="ParameterLevel"/>
    /// </summary>
    public void AddParametersFor(List<ITableInfo> tableInfos)
    {
        AddParametersFor(tableInfos.ToArray(), ParametersFoundSoFarInQueryGeneration[ParameterLevel.TableInfo]);
    }

    /// <summary>
    /// Records parameters from the <see cref="TableInfo"/> at the appropriate <see cref="ParameterLevel"/>
    /// </summary>
    public void AddParametersFor(ITableInfo tableInfo)
    {
        AddParametersFor(tableInfo, ParametersFoundSoFarInQueryGeneration[ParameterLevel.TableInfo]);
    }

    /// <summary>
    /// Records parameters from the <see cref="IFilter"/> at the appropriate <see cref="ParameterLevel"/>
    /// </summary>
    public void AddParametersFor(List<IFilter> filters)
    {
        AddParametersFor(filters.ToArray(), ParametersFoundSoFarInQueryGeneration[ParameterLevel.QueryLevel]);
    }

    /// <summary>
    /// Records parameters from the <see cref="ICollectSqlParameters"/> at the specified <see cref="ParameterLevel"/>
    /// </summary>
    public void AddParametersFor(ICollectSqlParameters collector, ParameterLevel parameterLevel)
    {
        AddParametersFor(collector, ParametersFoundSoFarInQueryGeneration[parameterLevel]);
    }

    /// <summary>
    /// Adds a new global parameter which will overridde other parameters declared at lower <see cref="ParameterLevel"/>
    /// 
    /// <para>The <see cref="State"/> must be <see cref="ParameterManagerLifecycleState.AllowingGlobals"/> for this to be allowed</para>
    /// </summary>
    /// <param name="parameter"></param>
    public void AddGlobalParameter(ISqlParameter parameter)
    {
        if (State != ParameterManagerLifecycleState.AllowingGlobals)
            throw new InvalidOperationException(
                "Cannot add global parameters at this stage, State must be AllowingGlobals.  Basically you can only add globals to a QueryBuilder before it has ever generated any .SQL, to prevent duplication or out dated results vs the results of a Resolved SQL resultant query");

        ParametersFoundSoFarInQueryGeneration[ParameterLevel.Global].Add(parameter);
    }

    private void AddParametersFor(ICollectSqlParameters[] collectors, List<ISqlParameter> toAddTo)
    {
        if (!collectors.Any())
            return;

        foreach (var collector in collectors)
            AddParametersFor(collector, toAddTo);
    }

    private void AddParametersFor(ICollectSqlParameters collector, List<ISqlParameter> toAddTo)
    {
        if (State == ParameterManagerLifecycleState.Finalized)
            throw new InvalidOperationException(
                $"Cannot add new {collector.GetType().Name} level parameters because state is {State}");

        State = ParameterManagerLifecycleState.ParameterDiscovery;

        toAddTo.AddRange(collector.GetAllParameters());
    }


    /// <summary>
    /// Resolves all <see cref="ISqlParameter"/> found so far and merges/overrides as appropriate to accommodate identical side by side parameters and
    /// global overriding ones etc.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<ISqlParameter> GetFinalResolvedParametersList()
    {
        State = ParameterManagerLifecycleState.Finalized;

        var toReturn = new List<ParameterFoundAtLevel>();

        foreach (var kvp in ParametersFoundSoFarInQueryGeneration)
            foreach (var sqlParameter in kvp.Value)
                AddParameterToCollection(new ParameterFoundAtLevel(sqlParameter, kvp.Key), toReturn);


        //There can be empty parameters during resolution but only if it finds an overriding one further up the hierarchy
        var emptyParameter = toReturn.FirstOrDefault(t => string.IsNullOrWhiteSpace(t.Parameter.Value));
        if (emptyParameter != null)
        {
            var exceptionMessage = $"No Value defined for Parameter {emptyParameter.Parameter.ParameterName}";
            var asConcreteObject = emptyParameter.Parameter as IMapsDirectlyToDatabaseTable ??
                                   throw new QueryBuildingException(exceptionMessage);

            //problem was from a user one from their Catalogue Database, tell them the ProblemObject as well
            throw new QueryBuildingException(exceptionMessage, new[] { asConcreteObject });
        }

        return toReturn.Select(t => t.Parameter);
    }

    /// <summary>
    /// Removes all non global parameters from the <see cref="ParameterManager"/> and returns the <see cref="State"/> to allow new parameters
    /// </summary>
    public void ClearNonGlobals()
    {
        ParametersFoundSoFarInQueryGeneration[ParameterLevel.CompositeQueryLevel].Clear();
        ParametersFoundSoFarInQueryGeneration[ParameterLevel.QueryLevel].Clear();
        ParametersFoundSoFarInQueryGeneration[ParameterLevel.TableInfo].Clear();
        State = ParameterManagerLifecycleState.ParameterDiscovery;
        _memoryRepository.Clear();
    }

    private static void AddParameterToCollection(ParameterFoundAtLevel toAdd,
        List<ParameterFoundAtLevel> existingParameters)
    {
        //see if parameter if we already have one with the same name
        var duplicate = existingParameters.FirstOrDefault(p =>
            p.Parameter.ParameterName.Equals(toAdd.Parameter.ParameterName,
                StringComparison.InvariantCultureIgnoreCase));
        if (duplicate != null)
        {
            //deal with duplicate parameter naming e.g. @startDate BUT: declared with 2 different types
            if (
                    !toAdd.Parameter.ParameterSQL.Trim()
                        .Equals(duplicate.Parameter.ParameterSQL.Trim(), StringComparison.CurrentCultureIgnoreCase))
                //to lower them so that we don't complain about 'AS VARCHAR(50)' vs 'as varchar(50)'
                ThrowExceptionForParameterPair(
                    $"Found multiple parameters called {toAdd.Parameter} but with differing SQL:{toAdd.Parameter.ParameterSQL} vs {duplicate.Parameter.ParameterSQL}",
                    toAdd, duplicate);


            //if values differ!
            if (!string.Equals((duplicate.Parameter.Value ?? "").Trim(), (toAdd.Parameter.Value ?? "").Trim(),
                    StringComparison.CurrentCultureIgnoreCase))
            {
                //if the duplicate (already existing) parameter is of a lower level then it can be discarded because it didn't have a dodgy type mismatch etc (see ThrowIfUnsuitable above)
                if (duplicate.Level < toAdd.Level)
                {
                    existingParameters.Remove(duplicate);
                    existingParameters.Add(toAdd);
                }
                else
                {
                    ThrowExceptionForParameterPair(
                        $"Found 2+ parameters with the name {toAdd} but differing Values of \"{toAdd.Parameter.Value}\" and \"{duplicate.Parameter.Value}\"",
                        toAdd, duplicate);
                }
            }
            //if we get here then its a duplicate but it is an exact duplicate so don't worry
        }
        else
        {
            existingParameters.Add(toAdd); //its not a duplicate so add it to the list of RequiredParameters
        }
    }

    private static void ThrowExceptionForParameterPair(string exceptionMessage, ParameterFoundAtLevel parameter1,
        ParameterFoundAtLevel parameter2)
    {
        var concreteObjects = new List<IMapsDirectlyToDatabaseTable>();

        var desc1 = $"(Type:{parameter1.Parameter.GetType()}";
        var desc2 = $"(Type:{parameter2.Parameter.GetType()}";


        if (parameter1.Parameter is IMapsDirectlyToDatabaseTable concrete1)
        {
            concreteObjects.Add(concrete1);
            desc1 += $" ID:{concrete1.ID}";
        }

        if (parameter2.Parameter is IMapsDirectlyToDatabaseTable concrete2)
        {
            concreteObjects.Add(concrete2);
            desc2 += $" ID:{concrete2.ID}";
        }

        desc1 += ")";
        desc2 += ")";

        exceptionMessage += $".  Problem objects were {parameter1}{desc1} and {parameter2} {desc2}";

        throw new QueryBuildingException(exceptionMessage, concreteObjects);
    }

    /// <summary>
    /// Imports all TableInfo level parameters into a super set (with all TableInfo level parameters from every manager you have imported).  Also imports all
    /// QueryLevel parameters but for these it will do renames where there are conflicting named parameters, you must
    /// 
    /// </summary>
    /// <param name="toImport"></param>
    /// <param name="parameterNameSubstitutions"></param>
    public void ImportAndElevateResolvedParametersFromSubquery(ParameterManager toImport,
        out Dictionary<string, string> parameterNameSubstitutions)
    {
        if (toImport == this)
            throw new InvalidOperationException("Cannot import parameters into yourself!");

        if (State == ParameterManagerLifecycleState.Finalized)
            throw new InvalidOperationException(
                $"Cannot import parameters because state of ParameterManager is already {ParameterManagerLifecycleState.Finalized}");

        if (toImport.ParametersFoundSoFarInQueryGeneration[ParameterLevel.CompositeQueryLevel].Any())
            throw new ArgumentException(
                $"Cannot import from ParameterManager because it has 1+ {ParameterLevel.CompositeQueryLevel} parameters in it too!");

        parameterNameSubstitutions = new Dictionary<string, string>();

        ////////////////////////////////////////////////////////////Handle TableInfo level parameters//////////////////////////////////////
        //for each table valued parameter (TableInfo level)
        foreach (var parameterToImport in toImport.ParametersFoundSoFarInQueryGeneration[ParameterLevel.TableInfo])
            //it does not already exist
            if (!ParametersFoundSoFarInQueryGeneration[ParameterLevel.CompositeQueryLevel].Any(p =>
                    p.ParameterName.Equals(parameterToImport.ParameterName, StringComparison.CurrentCultureIgnoreCase)))
                ParametersFoundSoFarInQueryGeneration[ParameterLevel.TableInfo].Add(parameterToImport); //import it
        //Do not handle renaming here because it is likely the user doesn't even know this parameter exists as it is a tableinfo level one i.e. a default they declared when they first imported their table valued function (or there is a QueryLevel override anyway)
        toImport.ParametersFoundSoFarInQueryGeneration[ParameterLevel.TableInfo].Clear();
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


        //////////////////////////////////////////////////////Handle all the other parameters//////////////////////////////////////////////
        //for each Query Level parameter
        foreach (var parameterToImport in toImport.GetFinalResolvedParametersList())
        {
            var toImportParameterName = parameterToImport.ParameterName;
            var existing = ParametersFoundSoFarInQueryGeneration[ParameterLevel.CompositeQueryLevel]
                .SingleOrDefault(p =>
                    p.ParameterName.Equals(toImportParameterName, StringComparison.CurrentCultureIgnoreCase));

            if (existing == null)
            {
                ParametersFoundSoFarInQueryGeneration[ParameterLevel.CompositeQueryLevel]
                    .Add(parameterToImport); //import it to the composite level
            }
            else
            {
                //we are importing SqlParameters from a subquery and we have found a parameter with the same name as one that is already contained at this composite level

                //if the one we are importing is 100% identical (same declaration and value) then we don't need to import it
                if (AreIdentical(existing, parameterToImport))
                    continue; //skip it

                //it is different so we have to handle the conflict.  This could be because user has a filter @icdCode in 2 datasets with 2 different meanings and 2 different values

                //however! if we have a global override configured for this parameter
                var overridingGlobal = GetOverrideIfAnyFor(existing);

                //with the same declaration SQL then we can discard the parameter because all values are going to be replaced by the global anyway!
                if (overridingGlobal != null)
                    if (AreDeclaredTheSame(overridingGlobal, parameterToImport))
                        continue; //override will replace both so don't bother importing it
                    else
                        //there's an override with the same name but different datatypes (that's a problem)
                        throw new QueryBuildingException(
                            $"Parameter {parameterToImport} has the same name as an existing parameter with a global override but differing declarations (normally we would handle with a rename but we can't because of the overriding global)",
                            new object[] { existing, parameterToImport, overridingGlobal });

                //one already exists so we will have to do a parameter rename

                //get the next number going e.g. _2 or _3 etc
                var newSuffix = GetSuffixForRenaming(toImportParameterName);

                //Add the rename operation to the audit
                parameterNameSubstitutions.Add(toImportParameterName, $"{parameterToImport.ParameterName}_{newSuffix}");

                //do the rename operation into a spontaneous object because modifying the ISqlParameter directly could corrupt it for other users (especially if SuperCaching is on! See RDMPDEV-668)
                var spont = new SpontaneouslyInventedSqlParameter(
                    _memoryRepository,
                    parameterToImport.ParameterSQL.Replace(toImportParameterName,
                        $"{parameterToImport.ParameterName}_{newSuffix}"),
                    parameterToImport.Value,
                    parameterToImport.Comment,
                    parameterToImport.GetQuerySyntaxHelper()
                );

                //now make it a composite query level parameter used by us
                ParametersFoundSoFarInQueryGeneration[ParameterLevel.CompositeQueryLevel].Add(spont);
            }
        }
    }

    private static bool AreDeclaredTheSame(ISqlParameter first, ISqlParameter other)
    {
        if (first == null || other == null)
            throw new NullReferenceException("You cannot pass null parameters into this method");

        var sql1 = first.ParameterSQL ?? "";
        var sql2 = other.ParameterSQL ?? "";

        return sql1.Trim().Equals(sql2.Trim(), StringComparison.CurrentCultureIgnoreCase);
    }

    private static bool AreIdentical(ISqlParameter first, ISqlParameter other)
    {
        var sameSql = AreDeclaredTheSame(first, other);

        var value1 = first.Value ?? "";
        var value2 = other.Value ?? "";

        var sameValue = value1.Trim().Equals(value2.Trim(), StringComparison.CurrentCultureIgnoreCase);

        return sameSql && sameValue;
    }

    private int GetSuffixForRenaming(string toImportParameterName)
    {
        //start at 2
        var counter = 2;


        //while we have parameter called @p_2, @p_3 etc etc keep adding
        while (
            ParametersFoundSoFarInQueryGeneration[ParameterLevel.CompositeQueryLevel].Any(
                p => p.ParameterName.Equals($"{toImportParameterName}_{counter}",
                    StringComparison.CurrentCultureIgnoreCase)))
            counter++;

        //we have now found a unique number
        return counter;
    }

    /// <summary>
    /// Returns all <see cref="ISqlParameter"/> which would be overwritten (ignored) because of higher level parameters during <see cref="GetFinalResolvedParametersList"/>
    /// 
    /// <para>Does not change the <see cref="State"/>of the <see cref="ParameterManager"/></para>
    /// </summary>
    /// <returns></returns>
    public ISqlParameter[] GetOverridenParameters()
    {
        var toReturn = new List<ISqlParameter>();

        var levels = (ParameterLevel[])Enum.GetValues(typeof(ParameterLevel));

        //for each level
        for (var i = 0; i < levels.Length; i++)
        {
            var currentLevel = levels[i];

            //for each parameter
            foreach (var p1 in ParametersFoundSoFarInQueryGeneration[currentLevel])
                //for each level above this
                for (var j = i + 1; j < levels.Length; j++)
                {
                    var comparisonLevel = levels[j];

                    //if there is a parameter at the above level with the same declaration
                    if (ParametersFoundSoFarInQueryGeneration[comparisonLevel].Any(p => AreDeclaredTheSame(p1, p)))
                        if (!toReturn.Contains(p1))
                            toReturn.Add(
                                p1); //it overrides this one (regardless of value - type differences do not result in overriding, they result in Exceptions! - see GetFinalResolvedParametersList)
                }
        }

        return toReturn.ToArray();
    }

    /// <summary>
    /// Returns the <see cref="ISqlParameter"/> which would be overwritten (ignored) because of higher level parameters during <see cref="GetFinalResolvedParametersList"/>
    /// </summary>
    /// <param name="existing"></param>
    /// <returns>The overriding parameter or null if there are none</returns>
    public ISqlParameter GetOverrideIfAnyFor(ISqlParameter existing)
    {
        var currentLevel = GetLevelForParameter(existing);

        var overrides = GetOverridenParameters();

        foreach (ParameterLevel level in Enum.GetValues(typeof(ParameterLevel)))
            if (level > currentLevel)
            {
                var compatibleOverride = ParametersFoundSoFarInQueryGeneration[level]
                    .FirstOrDefault(o => AreDeclaredTheSame(existing, o));

                //there are no override compatible parameters at this candidate level or the override is itself overridden at a higher level
                if (compatibleOverride == null || overrides.Contains(compatibleOverride))
                    continue;

                return compatibleOverride;
            }


        //no overrides
        return null;
    }

    /// <summary>
    /// Makes the <see cref="ParameterManager"/> forget about the given <see cref="ISqlParameter"/>
    /// 
    /// <para>This operation ignores <see cref="State"/> so you should not use the <see cref="ParameterManager"/> for code generation after calling this method</para>
    /// </summary>
    /// <param name="deletable"></param>
    public void RemoveParameter(ISqlParameter deletable)
    {
        foreach (var parameters in ParametersFoundSoFarInQueryGeneration.Values)
            if (parameters.Contains(deletable))
                parameters.Remove(deletable);
    }

    /// <summary>
    /// Returns the <see cref="ParameterLevel"/> that the <see cref="ISqlParameter"/> was found at or null if has not been added to this <see cref="ParameterManager"/>
    /// </summary>
    /// <param name="parameter"></param>
    /// <returns></returns>
    public ParameterLevel? GetLevelForParameter(ISqlParameter parameter)
    {
        return ParametersFoundSoFarInQueryGeneration.Any(k => k.Value.Contains(parameter))
            ? ParametersFoundSoFarInQueryGeneration
                //take the bottom most level it was found at
                .OrderBy(static kvp => kvp.Key)
                .First(k => k.Value.Contains(parameter)).Key
            : null;
    }

    /// <summary>
    /// Creates a shallow copy of the <see cref="ParameterManager"/> in which <see cref="ParametersFoundSoFarInQueryGeneration"/> is a new
    /// instance but the parameters referenced are shared (with the original).
    /// </summary>
    /// <returns></returns>
    public ParameterManager Clone()
    {
        var clone = new ParameterManager(ParametersFoundSoFarInQueryGeneration[ParameterLevel.Global].ToArray())
        {
            State = State
        };

        foreach (var kvp in ParametersFoundSoFarInQueryGeneration)
            clone.ParametersFoundSoFarInQueryGeneration[kvp.Key].AddRange(kvp.Value);

        return clone;
    }

    /// <summary>
    /// Returns all <see cref="ISqlParameter"/> that collide with <paramref name="other"/> (same name different value)
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public string[] GetCollisions(ParameterManager other)
    {
        var pm = new ParameterManager();
        pm.ImportAndElevateResolvedParametersFromSubquery(this, out var subs);

        //not sure how there could be collisions given we went into a fresh one but I guess it would count
        if (subs.Keys.Any())
            return subs.Keys.ToArray();

        pm.ImportAndElevateResolvedParametersFromSubquery(other, out subs);

        return subs.Keys.ToArray();
    }

    /// <summary>
    /// Imports novel <see cref="ISqlParameter"/> from <paramref name="other"/> without renaming.
    /// </summary>
    /// <param name="other"></param>
    /// <exception cref="QueryBuildingException">Thrown if there are non identical parameter name collisions (same name different value)</exception>
    public void MergeWithoutRename(ParameterManager other)
    {
        var collisions = GetCollisions(other);
        if (collisions.Any())
            throw new QueryBuildingException(
                $"PatientIndexTables cannot have parameters with the same name as their users.  Offending parameter(s) were {string.Join(",", collisions)}");

        foreach (ParameterLevel l in Enum.GetValues(typeof(ParameterLevel)))
        {
            var to = ParametersFoundSoFarInQueryGeneration[l];
            var from = other.ParametersFoundSoFarInQueryGeneration[l];

            //add all parameters which are not already represented with an identical parameter
            to.AddRange(from.Where(f => !to.Any(t => AreIdentical(f, t))));
        }
    }
}