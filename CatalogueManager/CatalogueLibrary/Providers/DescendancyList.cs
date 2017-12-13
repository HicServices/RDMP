using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CatalogueLibrary.Providers
{
    /// <summary>
    /// Audit of parents for a given object in the CatalogueChildProvider hierarchy that is used to populate RDMPCollectionUIs.  Every object that is not a root level 
    /// object will have a DescendancyList.  Normally any DatabaseEntity (or node class) has only one DescendancyList (path to reach it) however you can flag BetterRouteExists
    /// on a DescendancyList to indicate that if another DescendancyList is found for the object then that one is to be considered 'better' and used instead.  For example
    /// AggregateConfigurations which are modelling a cohort apper both under their respective Catalogue and their CohortIdentificationConfiguration but sometimes one is an
    /// orphan (it's CohortIdentificationConfiguration has been deleted or it has been removed from it) in which case the only path is the 'less goood' one.
    /// 
    /// It is not allowed to have duplicate objects in Parents.  All objects and parents must have appropriate implements of GetHashCode.
    /// </summary>
    public class DescendancyList
    {
        public object[] Parents;

        /// <summary>
        /// Set to true to indicate that you might find a better DescendancyList for the given object and if so that other DescendancyList should be considered 'better'
        /// </summary>
        public bool BetterRouteExists { get; private set; }

        public DescendancyList(params object[] parents)
        {
            Parents = parents;
        }

        public bool IsEmpty { get { return !Parents.Any(); } }

        /// <summary>
        /// Returns a new instance of DescendancyList that includes the new parent appended to the end of parent hierarchy. You can only add to the end so 
        /// if you have Root=>Grandparent then the only thing you should add is Parent.
        /// </summary>
        /// <param name="anotherKnownParent"></param>
        /// <returns></returns>
        public DescendancyList Add(object anotherKnownParent)
        {
            if(Parents.Contains(anotherKnownParent))
                throw new ArgumentException("DecendancyList already contains '" + anotherKnownParent + "'");

            var list = new List<object>(Parents);
            list.Add(anotherKnownParent);
            var toReturn = new DescendancyList(list.ToArray());
            toReturn.BetterRouteExists = BetterRouteExists;
            return toReturn;
        }

        /// <summary>
        /// Returns a new DescendancyList with BetterRouteExists set to true, this means the system will bear in mind it might see a better DescendancyList later on
        /// in which case it will use that better route instead
        /// </summary>
        /// <returns></returns>
        public DescendancyList SetBetterRouteExists()
        {
            var toReturn= new DescendancyList(Parents);
            toReturn.BetterRouteExists = true;
            return toReturn;
        }


        public override string ToString()
        {
            return "<<"+ string.Join("=>", Parents) + ">>";
        }

        /// <summary>
        /// returns the last object in the chain, for example Root=>GrandParent=>Parent would return 'Parent'
        /// </summary>
        /// <returns></returns>
        public object Last()
        {
            return Parents.Last();
        }
    }
}