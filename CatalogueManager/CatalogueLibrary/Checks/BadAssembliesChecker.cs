using System;
using System.Collections.Generic;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using ReusableLibraryCode.Checks;

namespace CatalogueLibrary.Checks
{
    /// <summary>
    /// Lists all plugin/dll load exceptions generated during Startup (when MEF is processed).  Also checks that all Types declared as ICheckable
    /// can be constructed
    /// </summary>
    public class BadAssembliesChecker : ICheckable
    {
        private readonly MEF _mefPlugins;

        /// <summary>
        /// Prepares to check the currently loaded assemblies defined in the MEF (Call CatalogueRepository.MEF to get the MEF), call Check to start the checking process
        /// </summary>
        /// <param name="mefPlugins"></param>
        public BadAssembliesChecker(MEF mefPlugins)
        {
            _mefPlugins = mefPlugins;
        }

        /// <summary>
        /// Lists assembly load errors and attempts to construct instances of all Types declared as Exports (which are ICheckable)
        /// </summary>
        /// <param name="notifier"></param>
        public void Check(ICheckNotifier notifier)
        {
            foreach (KeyValuePair<string, Exception> badAssembly in _mefPlugins.ListBadAssemblies())
                notifier.OnCheckPerformed(new CheckEventArgs("Could not load assembly " + badAssembly.Key, CheckResult.Fail, badAssembly.Value));

            foreach (Type t in _mefPlugins.GetAllTypes())
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Found Type " + t, CheckResult.Success, null));

                if (typeof(ICheckable).IsAssignableFrom(t))
                    try
                    {
                        _mefPlugins.FactoryCreateA<ICheckable>(t.FullName);
                    }
                    catch (Exception ex)
                    {
                        notifier.OnCheckPerformed(new CheckEventArgs(
                            "Class " + t.FullName +
                            " implements ICheckable but could not be created as an ICheckable.",
                            CheckResult.Warning, ex));
                    }
            }
        }
    }
}