using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MapsDirectlyToDatabaseTable;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Repositories;

namespace Rdmp.Core.CommandLine.Interactive.Picking
{
    /// <summary>
    /// Determines if a command line argument provided was a reference to one or more <see cref="DatabaseEntity"/> matching based on ID (e.g. "Catalogue:23")
    /// </summary>
    public class PickObjectByID :PickObjectBase
    {
        /*
            Console.WriteLine("Format \"\" e.g. \"Catalogue:*mysql*\" or \"Catalogue:12,23,34\"");

            */
        public override string Format => "{Type}:{ID}[,{ID2},{ID3}...]";
        public override string Help => 
@"Type: must be an RDMP object type e.g. Catalogue, Project etc.
ID: must reference an object that exists
ID2+: (optional) only allowed if you are being prompted for multiple objects, allows you to specify multiple objects of the same Type using comma separator";
        
        public override IEnumerable<string> Examples => new []
        {
            "Catalogue:1", 
            "Catalogue:1,2,3"
        };

        public override bool IsMatch(string arg, int idx)
        {
            var baseMatch = base.IsMatch(arg, idx);
            
            //only considered  match if the first letter is an Rdmp Type e.g. "Catalogue:12" but not "C:\fish"
            return baseMatch && 
                   RepositoryLocator.CatalogueRepository.MEF.GetType(Regex.Match(arg).Groups[1].Value) is Type t 
                   && typeof(IMapsDirectlyToDatabaseTable).IsAssignableFrom(t);
        }

        public PickObjectByID(IRDMPPlatformRepositoryServiceLocator repositoryLocator)
            :base(repositoryLocator,
                new Regex("^([A-Za-z]+):([0-9,]+)$",RegexOptions.IgnoreCase))
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