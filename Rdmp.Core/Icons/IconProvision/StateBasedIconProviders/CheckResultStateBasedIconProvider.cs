// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using SixLabors.ImageSharp;
using Rdmp.Core.Icons.IconProvision;
using ReusableLibraryCode.Checks;

namespace Rdmp.Core.Icons.IconProvision.StateBasedIconProviders
{
    public class CheckResultStateBasedIconProvider : IObjectStateBasedIconProvider
    {
        private Image _exception;
        private Image _warning;
        private Image _tick;

        public CheckResultStateBasedIconProvider()
        {
            _exception = CatalogueIcons.TinyRed;
            _warning = CatalogueIcons.TinyYellow;
            _tick = CatalogueIcons.TinyGreen;
        }
        
        public Image GetImageIfSupportedObject(object o)
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