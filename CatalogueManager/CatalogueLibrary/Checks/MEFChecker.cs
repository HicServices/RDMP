using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using ReusableLibraryCode.Checks;

namespace CatalogueLibrary.Checks
{
    public class MEFChecker : ICheckable
    {
        

        private string _classToFind;
        private readonly Action<string> _userAcceptedSubstitution;
        private MEF _mefPlugins;

        /// <summary>
        /// This class checks for the existence of a given MEF export e.g. [Export(typeof(IDataFlowSource))].  The class to find should be a fully expressed path to the class
        /// e.g. "DataExportLibrary.ExtractionTime.ExtractionPipeline.Sources.ExecuteDatasetExtractionSource" The class will not only check at runtime that the class
        /// exists but it will (via the ICheckNotifier) interface look for namespace changes that is classes with the same name as the missing MEF but in a different namespace
        /// 
        /// If the Check method finds a namespace change and the ICheckNotifier accepts the substitution then the Action userAcceptedSubstitution is called with the new class name
        /// 
        /// </summary>
        /// <param name="classToFind"></param>
        /// <param name="userAcceptedSubstitution"></param>
        public MEFChecker(MEF mefPlugins,string classToFind, Action<string> userAcceptedSubstitution)
        {
            _mefPlugins = mefPlugins;
            _classToFind = classToFind;
            _userAcceptedSubstitution = userAcceptedSubstitution;
        }

        public void Check(ICheckNotifier notifier)
        {
            if (string.IsNullOrWhiteSpace(_classToFind))
            {
                notifier.OnCheckPerformed(new CheckEventArgs(
                    "MEFChecker was asked to check for the existence of an Export class but the _classToFind string was empty",
                    CheckResult.Fail, null));
                return;
            }

            string typeNameOnly = _classToFind.Substring(_classToFind.LastIndexOf(".") + 1);

            var allTypes = _mefPlugins.GetAllTypes().ToArray();

            if (allTypes.Any(t => t.FullName.Equals(_classToFind)))
                notifier.OnCheckPerformed(new CheckEventArgs(
                    "Found MEF class " + _classToFind + "",
                    CheckResult.Success, null));
            else
            {

                Type[] substitute = allTypes.Where(t => t.Name.Equals(typeNameOnly)).ToArray();

                if (substitute.Length == 0)
                {

                    notifier.OnCheckPerformed(new CheckEventArgs(
                        "Could not find MEF class called " + _classToFind + " in LoadModuleAssembly.GetAllTypes() and couldn't even find any with the same basic name (Note that we only checked Exported MEF types e.g. classes implementing IPluginAttacher, IPluginDataProvider etc)",
                        CheckResult.Fail, null));

                    Dictionary<string, Exception> badAssemblies = _mefPlugins.ListBadAssemblies();

                    if (badAssemblies.Any())
                        notifier.OnCheckPerformed(new CheckEventArgs(
                            "It is possible that the class you are looking for is in the BadAssemblies list",
                            CheckResult.Fail, null));
                    foreach (KeyValuePair<string, Exception> badAssembly in badAssemblies)
                        notifier.OnCheckPerformed(new CheckEventArgs("Bad Assembly " + badAssembly.Key, CheckResult.Warning,
                            badAssembly.Value));
                }
                else if (substitute.Length == 1)
                {

                    bool acceptSubstitution = notifier.OnCheckPerformed(new CheckEventArgs(
                        "Could not find MEF class called " + _classToFind + " but did find one called " +
                        substitute[0].FullName,
                        CheckResult.Fail, null,
                        "Change reference to " + _classToFind + " to point to MEF assembly type " +
                        substitute[0].FullName));

                    if (acceptSubstitution)
                        _userAcceptedSubstitution(substitute[0].FullName);
                }
                else
                {
                    notifier.OnCheckPerformed(new CheckEventArgs(
                        "Could not find MEF class called " + _classToFind +
                        ", we were looking for a suitable replacment (a Type with the same basic name) but we found " +
                        substitute.Length + " subistutitions!!! (" +
                        substitute.Aggregate("", (s, n) => s + n.FullName + ","), CheckResult.Fail, null));
                }
            }
        }
    }
}
