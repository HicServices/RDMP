// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Drawing;
using CatalogueLibrary.Data.Cohort;

namespace CatalogueManager.Icons.IconProvision.StateBasedIconProviders
{
    public class CohortIdentificationConfigurationStateBasedIconProvider : IObjectStateBasedIconProvider
    {
        private Bitmap _cohortIdentificationConfiguration;
        private Bitmap _frozenCohortIdentificationConfiguration;

        public CohortIdentificationConfigurationStateBasedIconProvider()
        {
            _cohortIdentificationConfiguration = CatalogueIcons.CohortIdentificationConfiguration;
            _frozenCohortIdentificationConfiguration = CatalogueIcons.FrozenCohortIdentificationConfiguration;   
        }
        public Bitmap GetImageIfSupportedObject(object o)
        {
            var cic = o as  CohortIdentificationConfiguration;

            if (cic == null)
                return null;

            return cic.Frozen ? _frozenCohortIdentificationConfiguration : _cohortIdentificationConfiguration;

        }
    }
}