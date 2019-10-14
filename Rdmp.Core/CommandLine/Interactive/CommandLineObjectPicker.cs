using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MapsDirectlyToDatabaseTable;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Repositories;

namespace Rdmp.Core.CommandLine.Interactive
{
    /// <summary>
    /// Parses arguments given along with the "cmd" command to rdmp.exe (<see cref="Rdmp.Core.CommandLine.Options.ExecuteCommandOptions"/>).  This
    /// allows the user to launch certain commands (<see cref="Rdmp.Core.CommandExecution.BasicCommandExecution"/>) from the CLI.
    /// </summary>
    public class CommandLineObjectPicker
    {
        private readonly IRDMPPlatformRepositoryServiceLocator _repositoryLocator;
        Regex selectObjectByID = new Regex("([A-Za-z]+):([0-9,]+)");
        
        Regex selectObjectByToString = new Regex(@"([A-Za-z]+):([A-Za-z,*]+)");

        private CommandLineObjectPickerArgumentValue[] _arguments;

        public CommandLineObjectPickerArgumentValue this[int i] => _arguments[i];

        public CommandLineObjectPicker(IEnumerable<string> args,
            IRDMPPlatformRepositoryServiceLocator repositoryLocator)
        {
            _repositoryLocator = repositoryLocator;
            _arguments = args.Select(ParseValue).ToArray();
        }

        private CommandLineObjectPickerArgumentValue ParseValue(string arg,int idx)
        {
            var objByID = selectObjectByID.Match(arg);
            var objByToString = selectObjectByToString.Match(arg);

            if (objByID.Success)
            {
                string objectType = objByID.Groups[1].Value;
                string objectId = objByID.Groups[2].Value;

                Type dbObjectType = ParseDatabaseEntityType(objectType, arg, idx);
                
                var objs = objectId.Split(',').Select(id=>GetObjectByID(dbObjectType,int.Parse(id))).Distinct();
                
                return new CommandLineObjectPickerArgumentValue(arg,idx,objs.Cast<IMapsDirectlyToDatabaseTable>().ToArray());
            }

            if (objByToString.Success)
            {
                string objectType = objByToString.Groups[1].Value;
                string objectToString = objByToString.Groups[2].Value;

                Type dbObjectType = ParseDatabaseEntityType(objectType, arg, idx);
                
                var objs = objectToString.Split(',').SelectMany(str=>GetObjectByToString(dbObjectType,str)).Distinct();
                return new CommandLineObjectPickerArgumentValue(arg,idx,objs.Cast<IMapsDirectlyToDatabaseTable>().ToArray());
            }

            return new CommandLineObjectPickerArgumentValue(arg,idx);
        }

        private IEnumerable<IMapsDirectlyToDatabaseTable> GetObjectByToString(Type type, string pattern)
        {
            //build regex for the pattern which must be a complete match with anything (.*) matching the users wildcard
            Regex r = new Regex("^" + Regex.Escape(pattern).Replace(@"\*",".*") +"$");

            IEnumerable<IMapsDirectlyToDatabaseTable> toReturn;

            if(_repositoryLocator.CatalogueRepository.SupportsObjectType(type))
                toReturn = _repositoryLocator.CatalogueRepository.GetAllObjects(type);
            else
            if(_repositoryLocator.DataExportRepository.SupportsObjectType(type))
                toReturn = _repositoryLocator.DataExportRepository.GetAllObjects(type);
            else
                throw new ArgumentException("Did not know what repository to use to fetch objects of Type '" + type + "'");


            return toReturn.Where(m => r.IsMatch(m.ToString()));
        }

        public IMapsDirectlyToDatabaseTable GetObjectByID(Type type, int id)
        {
            if(_repositoryLocator.CatalogueRepository.SupportsObjectType(type))
                return _repositoryLocator.CatalogueRepository.GetObjectByID(type,id);
            if(_repositoryLocator.DataExportRepository.SupportsObjectType(type))
                return _repositoryLocator.DataExportRepository.GetObjectByID(type,id);
            
            throw new ArgumentException("Did not know what repository to use to fetch objects of Type '" + type + "'");
        }

        private Type ParseDatabaseEntityType(string objectType, string arg, int idx)
        {
            //todo c = Catalogue etc
            
            Type t = _repositoryLocator.CatalogueRepository.MEF.GetType(objectType);

            if(t == null)
                throw new CommandLineObjectPickerParseException("Could not recognize Type name",idx,arg);

            if(!typeof(DatabaseEntity).IsAssignableFrom(t))
                throw new CommandLineObjectPickerParseException("Type specified must be a DatabaseEntity",idx,arg);

            return t;


        }

        /// <summary>
        /// Returns true if the given <paramref name="idx"/> exists and is populated with a value of the expected <paramref name="paramType"/>
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="paramType"></param>
        /// <returns></returns>
        public bool HasArgumentOfType(int idx, Type paramType)
        {
            //if the index is greater than the number of arguments we have
            if (idx >= _arguments.Length)
                return false;

            return _arguments[idx].HasValueOfType(paramType);
        }
    }
}
