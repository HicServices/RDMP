using System;
using CatalogueLibrary.Data;

namespace CatalogueLibrary.DublinCore
{
    /// <summary>
    /// Handles updating / extracting data from RDMP objects using the interchange object DublinCoreDefinition
    /// </summary>
    public class DublinCoreTranslater
    {
        /// <summary>
        /// Populates the given <paramref name="toFill"/> with the descriptions stored in <paramref name="fillWith"/>.   This will overwrite previous values. 
        /// 
        /// <para>Not all object types T are supported</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="toFill"></param>
        /// <param name="fillWith"></param>
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

        /// <summary>
        /// Generates a <see cref="DublinCoreDefinition"/> for the provided <paramref name="generateFrom"/> by reading specific fields out of the object
        /// and translating them to dublin core metadata fields.
        /// 
        /// <para>Not all object types T are supported</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="generateFrom"></param>
        /// <returns></returns>
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
