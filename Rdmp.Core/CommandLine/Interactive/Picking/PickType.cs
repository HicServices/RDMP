using System.Collections.Generic;
using System.Text.RegularExpressions;
using Rdmp.Core.Repositories;

namespace Rdmp.Core.CommandLine.Interactive.Picking
{
    class PickType : PickObjectBase
    {
        public PickType(IRDMPPlatformRepositoryServiceLocator repositoryLocator) : base(repositoryLocator, new Regex(".*"))
        {
        }

        public override string Format { get; }
        public override string Help { get; }
        public override IEnumerable<string> Examples { get; }

        public override bool IsMatch(string arg, int idx)
        {
            return RepositoryLocator.CatalogueRepository.MEF.GetType(arg) != null;
        }

        public override CommandLineObjectPickerArgumentValue Parse(string arg, int idx)
        {
            return new CommandLineObjectPickerArgumentValue(arg,idx,RepositoryLocator.CatalogueRepository.MEF.GetType(arg));
        }
    }
}