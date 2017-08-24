using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CatalogueLibrary.Providers
{
    public class DescendancyList
    {
        public object[] Parents;
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