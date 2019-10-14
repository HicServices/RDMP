using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;

namespace Rdmp.Core.CommandLine.Interactive.Picking
{
    internal class PickObjectByName: PickObjectBase
    {
        public PickObjectByName(IRDMPPlatformRepositoryServiceLocator repositoryLocator) :
            base(repositoryLocator,
            new Regex(@"^([A-Za-z]+):([A-Za-z,*]+)$"))
        {
        }
        
        public override CommandLineObjectPickerArgumentValue Parse(string arg, int idx)
        {
            var objByToString = MatchOrThrow(arg, idx);
            
            string objectType = objByToString.Groups[1].Value;
            string objectToString = objByToString.Groups[2].Value;

            Type dbObjectType = ParseDatabaseEntityType(objectType, arg, idx);
            
            var objs = objectToString.Split(',').SelectMany(str=>GetObjectByToString(dbObjectType,str)).Distinct();
            return new CommandLineObjectPickerArgumentValue(arg,idx,objs.Cast<IMapsDirectlyToDatabaseTable>().ToArray());
        }

        private IEnumerable<object> GetObjectByToString(Type dbObjectType, string str)
        {
            return GetAllObjects(dbObjectType).Where(o => FilterByPattern(o, str));
        }
    }
}