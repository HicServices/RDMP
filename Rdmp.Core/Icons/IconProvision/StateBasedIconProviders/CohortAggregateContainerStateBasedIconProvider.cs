// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using SixLabors.ImageSharp;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Icons.IconProvision;

namespace Rdmp.Core.Icons.IconProvision.StateBasedIconProviders
{
    public class CohortAggregateContainerStateBasedIconProvider : IObjectStateBasedIconProvider
    {
        private Image _union;
        private Image _intersect;
        private Image _except;

        public CohortAggregateContainerStateBasedIconProvider()
        {
            _union = CatalogueIcons.UNION;
            _intersect = CatalogueIcons.INTERSECT;
            _except = CatalogueIcons.EXCEPT;            
        }
        public Image GetImageIfSupportedObject(object o)
        {
            if (o is Type && o.Equals(typeof (CohortAggregateContainer)))
                return _intersect;

            if (o is SetOperation)
                return GetImage((SetOperation) o);

            var container = o as CohortAggregateContainer;
            
            if (container == null)
                return null;

            return GetImage(container.Operation);
        }

        private Image GetImage(SetOperation operation)
        {
            switch (operation)
            {
                case SetOperation.UNION:
                    return _union;
                case SetOperation.INTERSECT:
                    return _intersect;
                case SetOperation.EXCEPT:
                    return _except;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}