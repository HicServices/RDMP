// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Repositories;
using RDMPStartup;
using ReusableUIComponents;

namespace CatalogueManager.TestsAndSetup.ServicePropogation
{
    public class ServiceLocatorPropagatorToChildControls
    {
        private readonly IRDMPPlatformRepositoryServiceLocator _repositoryLocator;
        public ServiceLocatorPropagatorToChildControls(IRepositoryUser root)
        {
            _repositoryLocator = root.RepositoryLocator;
        }
        public ServiceLocatorPropagatorToChildControls(IRDMPPlatformRepositoryServiceLocator repositoryLocator)
        {
            _repositoryLocator = repositoryLocator;
        }

        public void PropagateRecursively(IEnumerable<Control> controls ,bool visualStudioDesignMode)
        {
            foreach (Control control in controls)
            {
                var propagateLocator = control as RDMPUserControl;
                var propagateVisualStudioDesignerMode = control as IKnowIfImHostedByVisualStudio;

                if (propagateLocator != null)
                    propagateLocator.RepositoryLocator = _repositoryLocator;

                if(propagateVisualStudioDesignerMode != null)
                    propagateVisualStudioDesignerMode.SetVisualStudioDesignMode(visualStudioDesignMode);
                
                PropagateRecursively(control.Controls.Cast<Control>(),visualStudioDesignMode);

            }

            
        }
    }
}