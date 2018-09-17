using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataHelper;
using CatalogueLibrary.FilterImporting.Construction;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation;
using IFilter = CatalogueLibrary.Data.IFilter;

namespace CatalogueLibrary.FilterImporting
{
    /// <summary>
    /// Handles the creation of new ISqlParameters based on the current WHERE SQL of a given IFilter.  This involves parsing the WHERE SQL for variables (@myVar).  This class
    /// also takes into account any globals that exist and supports the use of template parameter values for new ISqlParameter created (importFromIfAny)
    /// 
    /// <para>globals are ISqlParameters which exist in the same scope as the IFilter being edited, if the WHERE Sql contains a parameter with the same name as a global then no new 
    /// ISqlParameter will be created.  For example if you have an IFilter "myfilter" with WhereSQL "@bob = 'bob'" and there are not already any parameters for the filter with the
    /// name @bob then a new one will be created (unless there is a global with the name @bob).</para>
    /// 
    /// <para>importFromIfAny is a collection of template parameters that contain appropriate values to assign to newly created parameters.  The use case for this is when you are importing
    /// a Catalogue filter (E.g. ExtractionFilter) into a lower level (e.g. DeployedExtractionFilter) and you want to propagate down all the appropriate parameters to the new level.
    /// In this use case the WhereSQL is parsed and any matching parameter names have the values copied into the newly created parameters.</para>
    /// 
    /// <para>This class relies on a delegate for creation of the actual parameter instances (See CreateAll method) </para>
    /// </summary>
    public class ParameterCreator
    {
        private readonly IFilterFactory _factory;
        private ISqlParameter[] _importFromIfAny;
        private ISqlParameter[] _globals;

        public ParameterCreator(IFilterFactory factory, IEnumerable<ISqlParameter> globals, ISqlParameter[] importFromIfAny)
        {
            if(globals != null)
                _globals = globals.ToArray();

            _factory = factory;
            _importFromIfAny = importFromIfAny;
        }

        public void CreateAll(IFilter filterToCreateFor, ISqlParameter[] existingParametersInScope)
        {
            //get what parameter exists
            ISqlParameter[] sqlParameters = filterToCreateFor.GetAllParameters()??new ISqlParameter[0];

            //all parameters in the Select SQL
            HashSet<string> parametersRequiredByWhereSQL = GetRequiredParamaterNamesForQuery(filterToCreateFor, _globals);

            //find which current parameters are redundant and delete them
            foreach (ISqlParameter parameter in sqlParameters)
                if (!parametersRequiredByWhereSQL.Contains(parameter.ParameterName))
                    ((IDeleteable)parameter).DeleteInDatabase();

            //find new parameters that we don't have
            foreach (string requiredParameterName in parametersRequiredByWhereSQL)
            {
                if (!sqlParameters.Any(p => p.ParameterName.Equals(requiredParameterName)))
                {
                    ISqlParameter matchingTemplateFilter = null;
                    ISqlParameter newParameter;

                    //now we might be in the process of cloning another IFilter in which case we want the filters to match the templates ones
                    if (_importFromIfAny != null)
                        matchingTemplateFilter = _importFromIfAny.SingleOrDefault(t => t.ParameterName.Equals(requiredParameterName));
                    
                    string proposedNewParameterName = requiredParameterName;
                    int proposedAliasNumber = 2;

                    //Figure out of there are any collisions with existing parameters
                    if(existingParametersInScope != null)
                        if(existingParametersInScope.Any(e => e.ParameterName.Equals(proposedNewParameterName)))//there is a conflict between the parameter you are importing and one that already exists in scope
                        {
                            while (existingParametersInScope.Any(e => e.ParameterName.Equals(proposedNewParameterName + proposedAliasNumber)))
                                proposedAliasNumber++;
                        
                            //Naming conflict has been resolved! (by adding the proposed alias number on) so record that this is the new name
                            proposedNewParameterName = proposedNewParameterName + proposedAliasNumber;
                        }
                    
                    //The final name is different e.g. bob2 instead of bob so propagate into the WHERE SQL of the filter
                    if (!proposedNewParameterName.Equals(requiredParameterName))
                    {
                        filterToCreateFor.WhereSQL = RenameParameterInSQL(filterToCreateFor.WhereSQL,requiredParameterName, proposedNewParameterName);
                        filterToCreateFor.SaveToDatabase();
                    }
                    
                    //If there is a matching Template Filter
                    if(matchingTemplateFilter != null)
                    {
                        string toCreate = matchingTemplateFilter.ParameterSQL;

                        //theres a rename requirement e.g. 'DECLARE @bob AS int' to 'DECLARE @bob2 AS int' because the existing scope already has a parameter called @bob
                        if (proposedNewParameterName != requiredParameterName)
                            toCreate = toCreate.Replace(requiredParameterName, proposedNewParameterName);
                        
                        //construct it as a match to the existing parameter declared at the template level (see below for full match propogation)
                        newParameter = _factory.CreateNewParameter(filterToCreateFor,toCreate);
                    }
                    else
                    {
                        var syntaxHelper = filterToCreateFor.GetQuerySyntaxHelper();
                        //its not got a template match so just create it as varchar(50)
                        var declaration = syntaxHelper.GetParameterDeclaration(proposedNewParameterName,new DatabaseTypeRequest(typeof(string),50));

                        newParameter = _factory.CreateNewParameter(filterToCreateFor,declaration);

                        if (newParameter != null)
                        {
                            newParameter.Value = "'todo'";
                            newParameter.SaveToDatabase();
                        }
                    }
                    if (newParameter == null)
                        throw new NullReferenceException("Parameter construction method returned null, expected it to return an ISqlParameter");

                 

                    //We have a template so copy across the remaining values
                    if (matchingTemplateFilter != null)
                    {
                        newParameter.Value = matchingTemplateFilter.Value;
                        newParameter.Comment = matchingTemplateFilter.Comment;
                        newParameter.SaveToDatabase();
                    }

                }
            }
        }

        /// <summary>
        /// Lists the names of all parameters required by the supplied whereSql e.g. @bob = 'bob' would return "@bob" unless 
        /// there is already a global parameter called @bob.  globals is optional, pass in null if there aren't any
        /// </summary>
        /// <param name="filter">the SQL filter you want to determine the parameter names in, does not have to include WHERE (infact probably shouldnt)</param>
        /// <param name="globals">optional parameter, an enumerable of parameters that already exist in a superscope (i.e. global parametetrs)</param>
        /// <returns>parameter names that are required by the SQL but are not already declared in the globals</returns>
        private HashSet<string> GetRequiredParamaterNamesForQuery(IFilter filter, IEnumerable<ISqlParameter> globals)
        {
            HashSet<string> toReturn = filter.GetQuerySyntaxHelper().GetAllParameterNamesFromQuery(filter.WhereSQL);

            //remove any global parameters (these don't need to be created)
            if (globals != null)
                foreach (ISqlParameter globalExtractionFilterParameter in globals)
                    if (toReturn.Contains(globalExtractionFilterParameter.ParameterName))
                        toReturn.Remove(globalExtractionFilterParameter.ParameterName);

            return toReturn;
        }

        public static string RenameParameterInSQL(string haystack, string parameterName, string parameterNameReplacement)
        {
            //Does a zero matching look ahead for anything that isn't a legal parameter name e.g. "@bob)" will match the bracket but will not replace the bracket since the match is zero width
            string regexBoundary = @"(?=[^A-Za-z0-9_#@$])";

            var patternNeedle = Regex.Escape(parameterName);

            //Replace the users needle with either an 'end of string' boundary or an invalid parameter name character (e.g. space, bracket, equals etc)
            var regex = new Regex("("+patternNeedle +"$)" + "|(" + patternNeedle + regexBoundary + ")");

            return regex.Replace(haystack, parameterNameReplacement);
        }
    }
}
