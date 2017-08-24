using System;
using System.Collections.Generic;
using System.Drawing;
using CatalogueLibrary.Data.PerformanceImprovement;
using ReusableLibraryCode.Checks;

namespace CatalogueManager.Icons.IconProvision.StateBasedIconProviders
{
    public class CheckResultStateBasedIconProvider : IObjectStateBasedIconProvider
    {
        private Bitmap _exception;
        private Bitmap _warning;
        private Bitmap _tick;

        public CheckResultStateBasedIconProvider()
        {
            _exception = FamFamFamIcons.flag_red;
            _warning = CatalogueIcons.Warning;
            _tick = CatalogueIcons.Tick;
        }
        
        public Bitmap GetImageIfSupportedObject(object o)
        {
            if (!(o is CheckResult))
                return null;

            switch ((CheckResult)o)
            {
                case CheckResult.Success:
                    return _tick;
                case CheckResult.Warning:
                    return _warning;
                case CheckResult.Fail:
                    return _exception;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}