using System;
using System.Linq;
using System.Text.RegularExpressions;
using MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;

namespace Rdmp.Core.CommandLine.Interactive.Picking
{
    internal class PickObjectByID :PickObjectBase
    {
        public PickObjectByID(IRDMPPlatformRepositoryServiceLocator repositoryLocator)
            :base(repositoryLocator,
                new Regex("^([A-Za-z]+):([0-9,]+)$"))
        {
                
        }

        public override CommandLineObjectPickerArgumentValue Parse(string arg, int idx)
        {
            var objByID = MatchOrThrow(arg, idx);

            string objectType = objByID.Groups[1].Value;
            string objectId = objByID.Groups[2].Value;

            Type dbObjectType = ParseDatabaseEntityType(objectType, arg, idx);
                
            var objs = objectId.Split(',').Select(id=>GetObjectByID(dbObjectType,int.Parse(id))).Distinct();
                
            return new CommandLineObjectPickerArgumentValue(arg,idx,objs.Cast<IMapsDirectlyToDatabaseTable>().ToArray());
        }
    }
}