using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.FilterImporting.Construction;
using Microsoft.Office.Interop.Word;

namespace CatalogueLibrary.FilterImporting
{
    public class FilterImporter
    {
        private readonly IFilterFactory _factory;
        private ISqlParameter[] _globals;

        public ISqlParameter[] AlternateValuesToUseForNewParameters { get; set; }

        public FilterImporter(IFilterFactory factory, ISqlParameter[] globals)
        {
            _factory = factory;
            _globals = globals;
        }
        
        public IFilter ImportFilter(IFilter fromMaster, IFilter[] existingFiltersAlreadyInScope)
        {
            var extractionFilter = fromMaster as ExtractionFilter;

            if(extractionFilter != null && extractionFilter.ExtractionInformation.ColumnInfo == null) 
                throw new Exception("Could not import filter "+extractionFilter+" because it could not be traced back to a ColumnInfo");

            //If user is trying to publish a filter into the Catalogue as a new master top level filter, make sure it is properly documented
            if (_factory is ExtractionFilterFactory)
            {
                string reason;
                if(!IsProperlyDocumented(fromMaster, out reason))
                    throw new Exception("Cannot clone filter called '"+fromMaster.Name+"' because:" + reason);
            }

            //Handle problems with existing filters
            existingFiltersAlreadyInScope = existingFiltersAlreadyInScope ?? new IFilter[0];

            if(existingFiltersAlreadyInScope.Contains(fromMaster))
                throw new ArgumentException("Master filter (that you are trying to import) cannot be part of the existing filters collection!");

            //Ensure that the new filter has a unique name within the scope
            string name = fromMaster.Name;
            
            while (existingFiltersAlreadyInScope.Any(f => f.Name.Equals(name)))
                name = "Copy of " + name;

            //create the filter 
            var newFilter = _factory.CreateNewFilter(name);
            
            //Now copy across all the values from the master
            newFilter.Description = fromMaster.Description;
            newFilter.IsMandatory = fromMaster.IsMandatory;
            newFilter.WhereSQL = fromMaster.WhereSQL;

            //if we are down cloning from a master ExtractionFilter so record that the new filter is 
            if(fromMaster is ExtractionFilter)
                newFilter.ClonedFromExtractionFilter_ID = fromMaster.ID;//make the new filters parent the master

            //if we are up cloning we are publishing a child into being a new master catalogue filter (ExtractionFilter) 
            if (newFilter is ExtractionFilter)
            {
                newFilter.Description += Environment.NewLine + " Published by " + Environment.UserName + " on " + DateTime.Now + " from object " + fromMaster.GetType().Name + " with ID " + fromMaster.ID;
                fromMaster.ClonedFromExtractionFilter_ID = newFilter.ID;//Make the newly created master our parent (since we are published)
            }

            newFilter.SaveToDatabase();

            //If there are some filters already in scope then we need to take into account their parameters when it comes to importing, so fetch a union of all the parameters
            var existingFiltersParametersAlreadyInScope = existingFiltersAlreadyInScope.SelectMany(f => f.GetAllParameters()).ToArray();

            //now create parameters while respecting globals
            var parameterCreator = new ParameterCreator(_factory, _globals, AlternateValuesToUseForNewParameters ?? fromMaster.GetAllParameters()); 
            parameterCreator.CreateAll(newFilter, existingFiltersParametersAlreadyInScope); //Create the parameters while handling the existing parameters in scope

            return newFilter;
        }


        /// <summary>
        /// Imports a collection of IFilters of one type into another type.  Destination type corresponds to the factory.  Returns the newly created filters.
        /// </summary>
        /// <param name="allMasters"></param>
        /// <param name="existingFiltersAlreadyInScope"></param>
        /// <returns></returns>
        public IFilter[] ImportAllFilters(IFilter[] allMasters, IFilter[] existingFiltersAlreadyInScope)
        {
            List<IFilter> createdSoFar = new List<IFilter>();

            existingFiltersAlreadyInScope = existingFiltersAlreadyInScope ?? new IFilter[0];

            foreach (IFilter master in allMasters)
            {
                var added = ImportFilter(master, createdSoFar.Union(existingFiltersAlreadyInScope).ToArray());
                createdSoFar.Add(added);
            }

            return createdSoFar.Except(existingFiltersAlreadyInScope).ToArray();
        }


        public static bool IsProperlyDocumented(IFilter filter, out string reason)
        {
            reason = null;

            if (String.IsNullOrWhiteSpace(filter.Description))
                reason = "There is no description";
            else
                if (filter.Description.Length <= 20)
                    reason = "Description is not long enough (minimum length is 20 characters)";
                else if (String.IsNullOrWhiteSpace(filter.WhereSQL))
                    reason = "WhereSQL is not populated";

            //if we have not yet found a reason to complain, look at parameters for a reason to complain
            if (reason == null)
            {
                //check to see if theres a problem with the parameters
                foreach (ISqlParameter filterParameter in filter.GetAllParameters())
                {
                    string reasonParameterRejected;
                    if (!ExtractionFilterParameter.IsProperlyDocumented(filterParameter, out reasonParameterRejected))
                    {
                        reason = "Parameter '" + filterParameter.ParameterName + "' was rejected :" + reasonParameterRejected;
                        break;
                    }
                }
            }

            return reason == null;
        }


        /*
          public ExtractionFilter CreateNewExtractionFilterAsPublishOf(IFilter toPublish, ExtractionInformation toAddTo)
        {
            string reason;
            if (!IsProperlyDocumented(toPublish,out reason))
                throw new Exception("Cannot clone filter called '"+toPublish.Name+"' because:"+reason);

            var clone = new ExtractionFilter(toAddTo.Repository, toPublish.Name, toAddTo);

            clone.WhereSQL = toPublish.WhereSQL;
            clone.IsMandatory = toPublish.IsMandatory;
            clone.Description = toPublish.Description + Environment.NewLine + " Published by " + Environment.UserName + " on " + DateTime.Now + " from object " + toPublish.GetType().Name + " with ID " + toPublish.ID;
            clone.SaveToDatabase();


            new ParameterCreator(new ExtractionFilterFactory(ExtractionInformation), )

            clone.CreateOrDeleteParametersBasedOnSQL(null,null);
            var clonedParams = clone.GetAllParameters(); 

            foreach (ISqlParameter p in toPublish.GetAllParameters())
            {
                var pclone = clonedParams.Single(c => c.ParameterName.Equals(p.ParameterName));

                pclone.Comment = p.Comment;
                pclone.ParameterSQL = p.ParameterSQL;
                pclone.Value = p.Value;
                pclone.SaveToDatabase();
            }


            //we have published it so make the link real to preserve modifications relationship that is normally set up when you import from catalogue (the other way around of doing things)
            toPublish.ClonedFromExtractionFilter_ID = clone.ID;
            return clone;
        }
         
         
        
         */
    }
}
