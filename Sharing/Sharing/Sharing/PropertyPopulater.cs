using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MapsDirectlyToDatabaseTable;

namespace Sharing.Sharing
{
    /// <summary>
    /// Handles populating database properties of IMapsDirectlyToDatabaseTable objects based on inputs such as MapsDirectlyToDatabaseTableStatelessDefinition
    /// or another object of the same type.
    /// </summary>
    public class PropertyPopulater
    {

        /// <summary>
        /// Copies all properties (Description etc) from one CatalogueItem into another (except ID properties).
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="skipNameProperty"></param>
        public void CopyNonIDValuesAcross(IMapsDirectlyToDatabaseTable from, IMapsDirectlyToDatabaseTable to, bool skipNameProperty = false)
        {
            var type = from.GetType();

            if(to.GetType() != type)
                throw new Exception("From and To objects must be of the same Type");
            
            foreach (var propertyInfo in type.GetProperties())
            {
                if (propertyInfo.Name == "ID")
                    continue;

                if (propertyInfo.Name.EndsWith("_ID"))
                    continue;

                if (propertyInfo.Name == "Name" && skipNameProperty)
                    continue;

                if (propertyInfo.CanWrite == false || propertyInfo.CanRead == false)
                    continue;

                object value = propertyInfo.GetValue(from, null);
                propertyInfo.SetValue(to, value, null);
            }

            var s = to as ISaveable;
            if(s != null)
                s.SaveToDatabase();
        }
    }
}
