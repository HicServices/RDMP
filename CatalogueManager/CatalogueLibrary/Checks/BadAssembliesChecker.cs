using System;
using System.Collections.Generic;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using ReusableLibraryCode.Checks;

namespace CatalogueLibrary.Checks
{
    public class BadAssembliesChecker : ICheckable
    {
        private readonly MEF _mefPlugins;

        public BadAssembliesChecker(MEF mefPlugins)
        {
            _mefPlugins = mefPlugins;
        }

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