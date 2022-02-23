// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using MapsDirectlyToDatabaseTable;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Repositories;
using Rdmp.Core.Validation.Constraints.Secondary;
using Rdmp.Core.Validation.Dependency.Exceptions;
using ReusableLibraryCode;

namespace Rdmp.Core.Validation.Dependency
{
    /// <summary>
    /// Prevents deleting objects which are referenced in the ValidatorXML of Catalogues.  This is done by processing the ValidatorXML as a string for speed
    /// rather than deserializing every ValidationXml.
    /// </summary>
    public class ValidationXMLObscureDependencyFinder:IObscureDependencyFinder
    {
        /// <summary>
        /// This is a list of regex patterns for identifying xml serialized classes that implement IMapsDirectlyToDatabaseTable in Xml strings
        /// It is used to detect when you are trying to delete an object which has hidden references to it in important serialized bits of 
        /// text (e.g. Catalogue.ValidationXML).
        /// </summary>
        public List<Suspect> TheUsualSuspects = new List<Suspect>();

        /// <summary>
        /// Catalogues whose ValidationXML doesn't resolve properly
        /// </summary>
        public List<Catalogue> CataloguesWithBrokenValidationXml = new List<Catalogue>();


        readonly MEF _mef;

        public ValidationXMLObscureDependencyFinder(ICatalogueRepositoryServiceLocator catalogueRepositoryServiceLocator)
        {
            if (Validator.LocatorForXMLDeserialization == null)
                Validator.LocatorForXMLDeserialization = catalogueRepositoryServiceLocator;
           
            _mef = catalogueRepositoryServiceLocator.CatalogueRepository.MEF;
            
        }

        private bool initialized = false;

        private void Initialize()
        {
            initialized = true;

            //get all the SecondaryConstraints
            foreach (Type constraintType in _mef.GetAllTypes().Where(c => typeof(ISecondaryConstraint).IsAssignableFrom(c)))
            {
                //get all properties and fields which map to a database object
                var props = constraintType.GetProperties().Where(p => typeof(IMapsDirectlyToDatabaseTable).IsAssignableFrom(p.PropertyType)).ToList();
                var fields = constraintType.GetFields().Where(f => typeof(IMapsDirectlyToDatabaseTable).IsAssignableFrom(f.FieldType)).ToList();

                //there are no suspect fields that could have hidden dependencies
                if (!props.Any() && !fields.Any())
                    continue;

                string constraintName = constraintType.Name;
                string pattern = Regex.Escape("<SecondaryConstraint xsi:type=\"" + constraintName + "\">");

                //anything
                pattern += ".*";

                //this will be replaced by the ID of the thing we are deleting (dont match 1 to 115 though!)
                pattern += @"\b{0}\b";

                //then more of anything
                pattern += ".*";

                //then the end of the secondary constraint
                pattern += Regex.Escape("</SecondaryConstraint>");

                TheUsualSuspects.Add(new Suspect(pattern, constraintType, props, fields));
            }
        }
        
        public void ThrowIfDeleteDisallowed(IMapsDirectlyToDatabaseTable oTableWrapperObject)
        {
            //dependency can only be initialized if mef is setup
            if(!initialized)
                if(_mef == null)
                    return;
                else
                    Initialize();

          ThrowIfDeleteDisallowed(oTableWrapperObject,0);
        }

        public void HandleCascadeDeletesForDeletedObject(IMapsDirectlyToDatabaseTable oTableWrapperObject)
        {
            
        }

        private void ThrowIfDeleteDisallowed(IMapsDirectlyToDatabaseTable oTableWrapperObject, int depth)
        {
            if (oTableWrapperObject == null)
                return;

            var repository = oTableWrapperObject.Repository;

            var treeObject = oTableWrapperObject as IHasDependencies;

            if (depth >= 5)//its fine
                return;

            if (treeObject != null)
            {
                IHasDependencies[] dependants;

                try
                {
                    dependants = treeObject.GetObjectsDependingOnThis();
                }
                catch (Exception)
                {
                    //couldn't get the dependants, we are mid delete operation so to be honest it's not super surprising if a dependency is unresolvable
                    dependants = null;
                }
                
                //check for undeletable dependants
                if (dependants != null)
                    foreach (IMapsDirectlyToDatabaseTable child in dependants.OfType<IMapsDirectlyToDatabaseTable>())
                        ThrowIfDeleteDisallowed(child, depth + 1);
            }

            //these regular expressions will let us identifiy suspicious Catalogues based on the validation
            List<Regex> checkers = new List<Regex>();

            foreach (Suspect suspect in TheUsualSuspects)
                checkers.Add(new Regex(string.Format(suspect.Pattern, oTableWrapperObject.ID), RegexOptions.Singleline));

            var firstPassSuspects = new HashSet<Catalogue>();

            //get all catalogues with some validation XML and see if the checker matches any of them
            foreach (Catalogue catalogue in repository.GetAllObjects<Catalogue>().Where(c => !string.IsNullOrWhiteSpace(c.ValidatorXML)))
                if (checkers.Any(checker => checker.IsMatch(catalogue.ValidatorXML)))
                    firstPassSuspects.Add(catalogue);

            foreach (Catalogue firstPassSuspect in firstPassSuspects)
                if (DeserializeToSeeIfThereIsADependency(oTableWrapperObject, firstPassSuspect))
                    throw new ValidationXmlDependencyException("The ValidationXML of Catalogue " + firstPassSuspect + " contains a reference to the object you are trying to delete:" + oTableWrapperObject);
        }

        private bool DeserializeToSeeIfThereIsADependency(IMapsDirectlyToDatabaseTable oTableWrapperObject, Catalogue firstPassSuspect)
        {
            //we already forbidlisted this Catalogue because it has dodgy XML that cant be deserialized properly
            var forbidlisted = CataloguesWithBrokenValidationXml.SingleOrDefault(c => c.ID == firstPassSuspect.ID);
            
            //it was forbidlisted because it had dodgy XML, if the xml hasn't changed it will still be broken so give up
            if(forbidlisted != null)
                if(forbidlisted.ValidatorXML.Equals(firstPassSuspect.ValidatorXML))
                    return false;
                else
                    CataloguesWithBrokenValidationXml.Remove(firstPassSuspect);//they have changed the ValidatorXML so maybe it is ok again

            //deserialize the catalogues validation XML
            Validator validator;
            try
            {
                validator = Validator.LoadFromXml(firstPassSuspect.ValidatorXML);
            }
            catch (Exception)
            {
                //add the newly identified dodgy Catalogue and add it to the forbidlist
                CataloguesWithBrokenValidationXml.Add(firstPassSuspect);
                return false;
            }

            //get all constraints
            IEnumerable<ISecondaryConstraint> constraints = validator.ItemValidators.SelectMany(iv => iv.SecondaryConstraints);

            //get those that are associated with the usual suspects
            foreach (ISecondaryConstraint constraint in constraints)
            {
                Suspect suspect = TheUsualSuspects.SingleOrDefault(s => s.Type == constraint.GetType());

                if (suspect == null)
                    continue;

                foreach (PropertyInfo p in suspect.SuspectProperties)
                    if (oTableWrapperObject.Equals(p.GetValue(constraint)))
                        return true;

                foreach (FieldInfo f in suspect.SuspectFields)
                    if (oTableWrapperObject.Equals(f.GetValue(constraint)))
                        return true;

            }

            return false;
        }
    }
}
