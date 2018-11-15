using System;
using CatalogueLibrary.Data;

namespace CatalogueLibrary.DublinCore
{
    /// <summary>
    /// Handles updating / extracting data from RDMP objects using the interchange object DublinCoreDefinition
    /// </summary>
    public class DublinCoreTranslater
    {
        public void Fill<T>(T toFill,DublinCoreDefinition fillWith)
        {
            Catalogue c = toFill as Catalogue;

            if (c != null)
            {
                c.Name = fillWith.Title;
                c.Description = fillWith.Description;
                c.Search_keywords = fillWith.Subject;
                c.Acronym = fillWith.Alternative;
            }
            else
                throw new NotSupportedException("Did not know how to hydrate the Type " + typeof(T) + " from a DublinCoreDefinition");
        }

        public DublinCoreDefinition GenerateFrom<T>(T generateFrom)
        {
            var toReturn = new DublinCoreDefinition();

            Catalogue c = generateFrom as Catalogue;

            if (c != null)
            {
                toReturn.Title = c.Name;
                toReturn.Description = c.Description;
                toReturn.Subject = c.Search_keywords;
                toReturn.Alternative = c.Acronym;
            }
            else
                throw new NotSupportedException("Did not know how to extracta a DublinCoreDefinition from the Type " + typeof(T));

            return toReturn;
        }
    }
}
