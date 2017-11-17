using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.Spontaneous;
using MapsDirectlyToDatabaseTable;

using IFilter = CatalogueLibrary.Data.IFilter;

namespace CatalogueLibrary.QueryBuilding.Parameters
{
    public class ParameterManager
    {
        public ParameterManagerLifecycleState State { get; private set; }

        public Dictionary<ParameterLevel,List<ISqlParameter>> ParametersFoundSoFarInQueryGeneration = new Dictionary<ParameterLevel, List<ISqlParameter>>();

        public ParameterManager(ISqlParameter[] globals = null)
        {
            State = ParameterManagerLifecycleState.AllowingGlobals;

            
            ParametersFoundSoFarInQueryGeneration.Add(ParameterLevel.TableInfo, new List<ISqlParameter>());
            ParametersFoundSoFarInQueryGeneration.Add(ParameterLevel.QueryLevel, new List<ISqlParameter>());
            ParametersFoundSoFarInQueryGeneration.Add(ParameterLevel.CompositeQueryLevel, new List<ISqlParameter>());
            ParametersFoundSoFarInQueryGeneration.Add(ParameterLevel.Global, new List<ISqlParameter>());

            if(globals != null)
                ParametersFoundSoFarInQueryGeneration[ParameterLevel.Global].AddRange(globals);
        }


        
        public void AddParametersFor(List<TableInfo> tableInfos)
        {
            AddParametersFor(tableInfos.ToArray(), ParametersFoundSoFarInQueryGeneration[ParameterLevel.TableInfo]);
        }

        public void AddParametersFor(TableInfo tableInfo)
        {
            AddParametersFor(tableInfo, ParametersFoundSoFarInQueryGeneration[ParameterLevel.TableInfo]);
        }
        public void AddParametersFor(List<IFilter> filters)
        {
            AddParametersFor(filters.ToArray(), ParametersFoundSoFarInQueryGeneration[ParameterLevel.QueryLevel]);
        }

        public void AddParametersFor(ICollectSqlParameters collector, ParameterLevel parameterLevel)
        {
            AddParametersFor(collector, ParametersFoundSoFarInQueryGeneration[parameterLevel]);
        }

        public void AddGlobalParameter(ISqlParameter parameter)
        {
            if(State != ParameterManagerLifecycleState.AllowingGlobals)
                throw new InvalidOperationException("Cannot add global parameters at this stage, State must be AllowingGlobals.  Basically you can only add globals to a QueryBuilder before it has ever generated any .SQL, to prevent duplication or out dated results vs the results of a Resolved SQL resultant query");

            ParametersFoundSoFarInQueryGeneration[ParameterLevel.Global].Add(parameter);
        }

        private void AddParametersFor(ICollectSqlParameters[] collectors, List<ISqlParameter> toAddTo)
        {
            if(!collectors.Any())
                return;

            foreach (ICollectSqlParameters collector in collectors)
                AddParametersFor(collector, toAddTo);
        }

        private void AddParametersFor(ICollectSqlParameters collector, List<ISqlParameter> toAddTo)
        {
            if (State == ParameterManagerLifecycleState.Finalized)
                throw new InvalidOperationException("Cannot add new " + collector.GetType().Name + " level parameters because state is " + State);

            State = ParameterManagerLifecycleState.ParameterDiscovery;

            toAddTo.AddRange(collector.GetAllParameters());
        }

        public IEnumerable<ISqlParameter> GetFinalResolvedParametersList()
        {
            State = ParameterManagerLifecycleState.Finalized;
            
            var toReturn = new List<ParameterFoundAtLevel>();

            foreach (KeyValuePair<ParameterLevel, List<ISqlParameter>> kvp in ParametersFoundSoFarInQueryGeneration)
                foreach (ISqlParameter sqlParameter in kvp.Value)
                    AddParameterToCollection(new ParameterFoundAtLevel(sqlParameter,kvp.Key), toReturn);
                

            //There can be empty parameters during resolution but only if it finds an overriding one further up the hierarchy
            var emptyParameter = toReturn.FirstOrDefault(t => string.IsNullOrWhiteSpace(t.Parameter.Value));
            if(emptyParameter != null)
            {
                string exceptionMessage = "No Value defined for Parameter " + emptyParameter.Parameter.ParameterName;
                var asConcreteObject = emptyParameter.Parameter as IMapsDirectlyToDatabaseTable;
                
                //problem was in a freaky parameter e.g. a constant one that doesn't come from database (rare to happen I would expect)
                if(asConcreteObject == null)
                    throw new QueryBuildingException(exceptionMessage);
                
                //problem was from a user one from their Catalogue Database, tell them the ProblemObject aswell
                throw new QueryBuildingException(exceptionMessage,new[]{asConcreteObject});
                
            }

            return toReturn.Select(t=>t.Parameter);
        }


        public void ClearNonGlobals()
        {
            ParametersFoundSoFarInQueryGeneration[ParameterLevel.CompositeQueryLevel].Clear();
            ParametersFoundSoFarInQueryGeneration[ParameterLevel.QueryLevel].Clear();
            ParametersFoundSoFarInQueryGeneration[ParameterLevel.TableInfo].Clear();
            State = ParameterManagerLifecycleState.ParameterDiscovery;
        }

        private void AddParameterToCollection(ParameterFoundAtLevel toAdd,List<ParameterFoundAtLevel> existingParameters)
        {
            //see if parameter if we already have one with the same name
            if (existingParameters.Any(p => p.Parameter.ParameterName == toAdd.Parameter.ParameterName))
            {
                ParameterFoundAtLevel duplicate = existingParameters.First(p => p.Parameter.ParameterName == toAdd.Parameter.ParameterName);
                
                //deal with duplicate paramater naming e.g. @startDate BUT: declared with 2 different types
                if (
                    !toAdd.Parameter.ParameterSQL.Trim()
                        .Equals(duplicate.Parameter.ParameterSQL.Trim(), StringComparison.InvariantCultureIgnoreCase))
                    //to lower them so that we don't complain about 'AS VARCHAR(50)' vs 'as varchar(50)'
                    ThrowExceptionForParameterPair("Found multiple parameters called " + toAdd.Parameter + " but with differing SQL:" + toAdd.Parameter.ParameterSQL + " vs " + duplicate.Parameter.ParameterSQL, toAdd, duplicate);


                //if values differ!
                if (!string.Equals((duplicate.Parameter.Value ?? "").Trim(),(toAdd.Parameter.Value??"").Trim(),StringComparison.InvariantCultureIgnoreCase))
                {
                    //if the duplicate (already existing) parameter is of a lower level then it can be discarded because it didn't have a dodgy type mismatch etc (see ThrowIfUnsuitable above)
                    if (duplicate.Level < toAdd.Level)
                    {
                        existingParameters.Remove(duplicate);
                        existingParameters.Add(toAdd);
                    }
                    else
                    ThrowExceptionForParameterPair("Found 2+ parameters with the name " + toAdd +
                                                     " but differing Values of \"" + toAdd.Parameter.Value + "\" and \"" +
                                                     duplicate.Parameter.Value + "\"",toAdd,duplicate);
                }
                //if we get here then its a duplicate but it is an exact duplicate so dont worry
            }
            else
                existingParameters.Add(toAdd); //its not a duplicate so add it to the list of RequiredParameters 
        }

        private void ThrowExceptionForParameterPair(string exceptionMessage, ParameterFoundAtLevel parameter1, ParameterFoundAtLevel parameter2)
        {
            var concrete1 = parameter1.Parameter as IMapsDirectlyToDatabaseTable;
            var concrete2 = parameter2.Parameter as IMapsDirectlyToDatabaseTable;

            List<IMapsDirectlyToDatabaseTable> concreteObjects = new List<IMapsDirectlyToDatabaseTable>();

            string desc1 = "(Type:" + parameter1.Parameter.GetType();
            string desc2 = "(Type:" + parameter2.Parameter.GetType();


            if(concrete1 != null)
            {
                concreteObjects.Add(concrete1);
                desc1 += " ID:" + concrete1.ID;
            }

            if(concrete2 != null)
            {

                concreteObjects.Add(concrete2);
                desc2 += " ID:" + concrete2.ID;
            }

            desc1 += ")";
            desc2 += ")";

            exceptionMessage += ".  Problem objects were " + parameter1 + desc1 + " and " + parameter2 + " " + desc2;
            
            throw new QueryBuildingException(exceptionMessage,concreteObjects);
        }

        /// <summary>
        /// Imports all TableInfo level paramaters into a super set (with all TableInfo level paramaters from every manager you have imported).  Also imports all
        /// QueryLevel parameters but for these it will do renames where there are conflicting named parameters, you must 
        /// 
        /// </summary>
        /// <param name="toImport"></param>
        public void ImportAndElevateResolvedParametersFromSubquery(ParameterManager toImport, out Dictionary<string,string> parameterNameSubstitutions)
        {
            if (toImport == this)
                throw new InvalidOperationException("Cannot import parameters into yourself!");

            if(State == ParameterManagerLifecycleState.Finalized)
                throw new InvalidOperationException("Cannot import parameters because state of ParameterManager is already " + ParameterManagerLifecycleState.Finalized);

            if(toImport.ParametersFoundSoFarInQueryGeneration[ParameterLevel.CompositeQueryLevel].Any())
                throw new ArgumentException("Cannot import from ParameterManager because it has 1+ " + ParameterLevel.CompositeQueryLevel +" parameters in it too!");
            
            parameterNameSubstitutions = new Dictionary<string, string>();

            ////////////////////////////////////////////////////////////Handle TableInfo level parameters//////////////////////////////////////
            //for each table valued parameter (TableInfo level)
            foreach (ISqlParameter parameterToImport in toImport.ParametersFoundSoFarInQueryGeneration[ParameterLevel.TableInfo])
            {
                //it does not already exist
                if (!ParametersFoundSoFarInQueryGeneration[ParameterLevel.CompositeQueryLevel].Any(p => p.ParameterName.Equals(parameterToImport.ParameterName)))
                    ParametersFoundSoFarInQueryGeneration[ParameterLevel.TableInfo].Add(parameterToImport); //import it 
                
                //Do not handle renaming here because it is likely the user doesn't even know this parameter exists as it is a tableinfo level one i.e. a default they declared when they first imported their table valued fuction (or there is a QueryLevel override anyway)
            }
            toImport.ParametersFoundSoFarInQueryGeneration[ParameterLevel.TableInfo].Clear();
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            

            //////////////////////////////////////////////////////Handle all the other parameters//////////////////////////////////////////////
            //for each Query Level parameter
            foreach (ISqlParameter parameterToImport in toImport.GetFinalResolvedParametersList())
            {
                string toImportParameterName = parameterToImport.ParameterName;
                var existing = ParametersFoundSoFarInQueryGeneration[ParameterLevel.CompositeQueryLevel].SingleOrDefault(p => p.ParameterName.Equals(toImportParameterName));
                
                if (existing == null)
                    ParametersFoundSoFarInQueryGeneration[ParameterLevel.CompositeQueryLevel].Add(parameterToImport);        //import it to the composite level
                else
                {
                    //we are importing SqlParameters from a subquery and we have found a parameter with the same name as one that is already contained at this composite level
                    
                    //if the one we are importing is 100% identical (same declaration and value) then we don't need to import it
                    if (AreIdentical(existing, parameterToImport))
                        continue;//skip it

                    //it is different so we have to handle the conflict.  This could be because user has a filter @icdCode in 2 datasets with 2 different meanings and 2 different values 

                    //however! if we have a global override configured for this parameter 
                    var overridingGlobal = GetOverrideIfAnyFor(existing);

                    //with the same declaration SQL then we can discard the parameter because all values are going to be replaced by the global anyway!
                    if(overridingGlobal != null)
                        if(AreDeclaredTheSame(overridingGlobal,parameterToImport))
                            continue;//override will replace both so don't bother importing it
                        else
                            //Theres an override with the same name but different datatypes (that's a problem)
                            throw new QueryBuildingException("Parameter " + parameterToImport + " has the same name as an existing parameter with a global override but differing declarations (normally we would handle with a rename but we can't because of the overriding global)",new object[]{existing,parameterToImport,overridingGlobal});

                    //one already exists so we will have to do a parameter rename

                    //get the next number going e.g. _2 or _3 etc
                    int newSuffix = GetSuffixForRenaming(toImportParameterName);

                    //Add the rename operation to the audit
                    parameterNameSubstitutions.Add(toImportParameterName, parameterToImport + "_" + newSuffix);

                    //do the rename operation into a spontaneous object because modifying the ISqlParameter directly could corrupt it for other users (especially if SuperCaching is on! See RDMPDEV-668)
                    var spont = new SpontaneouslyInventedSqlParameter(
                        parameterToImport.ParameterSQL.Replace(toImportParameterName,parameterToImport + "_" + newSuffix),
                        parameterToImport.Value,
                        parameterToImport.Comment
                        );
                    
                    //now make it a composite query level parameter used by us
                    ParametersFoundSoFarInQueryGeneration[ParameterLevel.CompositeQueryLevel].Add(spont);
                }
            }
        }
        
        private bool AreDeclaredTheSame(ISqlParameter first, ISqlParameter other)
        {
            if (first == null || other == null)
                throw new NullReferenceException("You cannot pass null parameters into this method");

            string sql1 = first.ParameterSQL ?? "";
            string sql2 = other.ParameterSQL ?? "";

            return sql1.Trim().Equals(sql2.Trim(), StringComparison.InvariantCultureIgnoreCase);
        }

        private bool AreIdentical(ISqlParameter first, ISqlParameter other)
        {
            bool sameSql = AreDeclaredTheSame(first, other);
            
            string value1 = first.Value ?? "";
            string value2 = other.Value??"";
            
            bool sameValue = value1.Trim().Equals(value2.Trim(), StringComparison.InvariantCultureIgnoreCase);

            return sameSql && sameValue;
        }

        private int GetSuffixForRenaming(string toImportParameterName)
        {
            //start at 2
            int counter = 2;


            //while we have parameter called @p_2, @p_3 etc etc keep adding
            while (
                ParametersFoundSoFarInQueryGeneration[ParameterLevel.CompositeQueryLevel].Any(
                    p => p.ParameterName.Equals(toImportParameterName + "_" + counter)))
                counter++;

            //we have now found a unique number
            return counter;
        }

        public ISqlParameter[] GetOverridenParameters()
        {
            List<ISqlParameter> toReturn = new List<ISqlParameter>();

            var levels = (ParameterLevel[])Enum.GetValues(typeof (ParameterLevel));

            //for each level
            for (int i = 0; i < levels.Length; i++)
            {
                var currentLevel = levels[i];

                //for each parameter
                foreach (var p1 in ParametersFoundSoFarInQueryGeneration[currentLevel])
                    //for each level above this
                    for (int j = i + 1; j < levels.Length; j++)
                    {
                        var comparisonLevel = levels[j];
                        
                        //if there is a parameter at the above level with the same declaration
                        if (ParametersFoundSoFarInQueryGeneration[comparisonLevel].Any(p => AreDeclaredTheSame(p1, p)))
                            if (!toReturn.Contains(p1))
                                toReturn.Add(p1);//it overrides this one (regardless of value - type differences do not result in overriding, they result in Exceptions! - see GetFinalResolvedParametersList)
                    }
            }

            return toReturn.ToArray();
        }

        public bool IsTopLevelOverride(ISqlParameter candidate)
        {
            var overridens = GetOverridenParameters();

            //it is itself overridden
            if (overridens.Contains(candidate))
                return false;

            return overridens.Any(o => AreDeclaredTheSame(o, candidate));
        }

        public ISqlParameter GetOverrideIfAnyFor(ISqlParameter existing)
        {
            var currentLevel = GetLevelForParameter(existing);

            var overrides = GetOverridenParameters();

            foreach (ParameterLevel level in Enum.GetValues(typeof (ParameterLevel)))
                if (level > currentLevel)
                {
                    var compatibleOverride = ParametersFoundSoFarInQueryGeneration[level].FirstOrDefault(o => AreDeclaredTheSame(existing, o));
                    
                    //there are no override compatible parameters at this candidate level or the override is itself overridden at a higher level
                    if (compatibleOverride == null || overrides.Contains(compatibleOverride))
                        continue;

                    return compatibleOverride;
                }


            //no overrides
            return null;
        }

        public void RemoveParameter(ISqlParameter deleteable)
        {
            foreach (List<ISqlParameter> parameters in ParametersFoundSoFarInQueryGeneration.Values)
                if (parameters.Contains(deleteable))
                    parameters.Remove(deleteable);
        }


        public ParameterLevel GetLevelForParameter(ISqlParameter parameter)
        {
            return 
                ParametersFoundSoFarInQueryGeneration.Single(k => k.Value.Contains(parameter)).Key;
        
        }
    }
}
