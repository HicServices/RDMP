using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MapsDirectlyToDatabaseTable;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Repositories;

namespace Rdmp.Core.CommandLine.Interactive.Picking
{
    public abstract class PickObjectBase
    {
        public abstract string Format { get; }
        public abstract string Help { get; }
        public abstract IEnumerable<string> Examples { get; }

        protected Regex Regex { get; }
        protected readonly IRDMPPlatformRepositoryServiceLocator RepositoryLocator;
        
        public virtual bool IsMatch(string arg, int idx)
        {
            return Regex.IsMatch(arg);
        }
        public abstract CommandLineObjectPickerArgumentValue Parse(string arg, int idx);


        /// <summary>
        /// Runs the <see cref="Regex"/> on the provided <paramref name="arg"/> throwing an <see cref="InvalidOperationException"/>
        /// if the match is a failure.
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="idx"></param>
        /// <returns></returns>
        protected Match MatchOrThrow(string arg, int idx)
        {
            var match = Regex.Match(arg);

            if(!match.Success)
                throw new InvalidOperationException("Regex did not match, no value could be parsed");

            return match;
        }

        public PickObjectBase(IRDMPPlatformRepositoryServiceLocator repositoryLocator,Regex regex)
        {
            Regex = regex;
            RepositoryLocator = repositoryLocator;
        }

        protected Type ParseDatabaseEntityType(string objectType, string arg, int idx)
        {
            //todo "c" = "Catalogue" etc
            
            Type t = RepositoryLocator.CatalogueRepository.MEF.GetType(objectType);

            if(t == null)
                throw new CommandLineObjectPickerParseException("Could not recognize Type name",idx,arg);

            if(!typeof(DatabaseEntity).IsAssignableFrom(t))
                throw new CommandLineObjectPickerParseException("Type specified must be a DatabaseEntity",idx,arg);

            return t;
        }
        protected IMapsDirectlyToDatabaseTable GetObjectByID(Type type, int id)
        {
            if(RepositoryLocator.CatalogueRepository.SupportsObjectType(type))
                return RepositoryLocator.CatalogueRepository.GetObjectByID(type,id);
            if(RepositoryLocator.DataExportRepository.SupportsObjectType(type))
                return RepositoryLocator.DataExportRepository.GetObjectByID(type,id);
            
            throw new ArgumentException("Did not know what repository to use to fetch objects of Type '" + type + "'");
        }


        protected IEnumerable<IMapsDirectlyToDatabaseTable> GetAllObjects(Type type)
        {
            IEnumerable<IMapsDirectlyToDatabaseTable> toReturn;

            if(RepositoryLocator.CatalogueRepository.SupportsObjectType(type))
                toReturn = RepositoryLocator.CatalogueRepository.GetAllObjects(type);
            else
            if(RepositoryLocator.DataExportRepository.SupportsObjectType(type))
                toReturn = RepositoryLocator.DataExportRepository.GetAllObjects(type);
            else
                throw new ArgumentException("Did not know what repository to use to fetch objects of Type '" + type + "'");

            return toReturn;
        }
        
        Dictionary<string,Regex> patternDictionary = new Dictionary<string, Regex>();

        /// <summary>
        /// Returns true if the <paramref name="pattern"/> (which is a simple non regex e.g. "Bio*") matches the ToString of <paramref name="o"/>
        /// </summary>
        /// <param name="o"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        protected bool FilterByPattern(object o, string pattern)
        {
            //build regex for the pattern which must be a complete match with anything (.*) matching the users wildcard
            if (!patternDictionary.ContainsKey(pattern))
                patternDictionary.Add(pattern, new Regex("^" + Regex.Escape(pattern).Replace(@"\*", ".*") + "$"));
            
            return patternDictionary[pattern].IsMatch(o.ToString());
        }
        
        /// <summary>
        /// Takes a key value pair in a string e.g. "Schema:dbo" and returns the substring "dbo".  Trims leading and trailing ':'.  Returns null if <paramref name="keyValueString"/> is null
        /// </summary>
        /// <param name="key"></param>
        /// <param name="keyValueString"></param>
        /// <returns></returns>
        protected string Trim(string key, string keyValueString)
        {
            if (string.IsNullOrWhiteSpace(keyValueString))
                return null;

            if(!keyValueString.StartsWith(key,StringComparison.CurrentCultureIgnoreCase))
                throw new ArgumentException($"Provided value '{keyValueString}' did not start with expected key '{key}'");

            return keyValueString.Substring(key.Length).Trim(':');
        }

        public virtual IEnumerable<string> GetAutoCompleteIfAny()
        {
            return null;
        }
    }
}