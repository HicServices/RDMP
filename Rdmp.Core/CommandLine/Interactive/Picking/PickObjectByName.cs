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
    /// Determines if a command line argument provided was a reference to one or more <see cref="DatabaseEntity"/> matching based on name (e.g. "Catalogue:my*cata")
    /// </summary>
    public class PickObjectByName: PickObjectBase
    {
        public override string Format => "{Type}:{NamePattern}[,{NamePattern2},{NamePattern3}...]";
        public override string Help => 
            @"Type: must be an RDMP object type e.g. Catalogue, Project etc.
NamePattern: must be a string that matches 1 (or more if selecting multiple objects) object based on it's name (ToString).  Can include the wild card '*'.  Cannot include the ':' character.
NamePattern2+: (optional) only allowed if you are being prompted for multiple objects, allows you to specify multiple objects of the same Type using comma separator";
        
        public override IEnumerable<string> Examples => new []
        {
            "Catalogue:mycata*", 
            "Catalogue:mycata1,mycata2"
        };

        public PickObjectByName(IRDMPPlatformRepositoryServiceLocator repositoryLocator) :
            base(repositoryLocator,
            new Regex(@"^([A-Za-z]+):([^:]+)$",RegexOptions.IgnoreCase))
        {
        }
        public override bool IsMatch(string arg, int idx)
        {
            var baseMatch = base.IsMatch(arg, idx);
            
            //only considered  match if the first letter is an Rdmp Type e.g. "Catalogue:12" but not "C:\fish"
            return baseMatch && 
                   RepositoryLocator.CatalogueRepository.MEF.GetType(Regex.Match(arg).Groups[1].Value) is Type t 
                   && typeof(IMapsDirectlyToDatabaseTable).IsAssignableFrom(t);
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