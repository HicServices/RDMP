// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Repositories;

namespace Rdmp.UI.PipelineUIs.DemandsInitializationUIs.ArgumentValueControls
{
    /// <summary>
    /// Constructor arguments for <see cref="IArgumentValueUI"/> implementations.  Records what property the control
    /// should allow editing (See <see cref="RequiredPropertyInfo"/>) and on what <see cref="IArgumentHost"/>.
    /// </summary>
    public class ArgumentValueUIArgs
    {
        public IArgumentHost Parent { get; set; }

        public object InitialValue { get; set; }
        public string ContextText { get; set; }
        public Type Type { get; set; }
        public RequiredPropertyInfo Required { get; set; }
        public ICatalogueRepository CatalogueRepository { get; set; }

        /// <summary>
        /// Call this when the value populated in the user interface is changed
        /// </summary>
        public Action<object> Setter { get; set; }

        /// <summary>
        /// Call this when the value populated in the user interface is illegal
        /// </summary>
        public Action<Exception> Fatal { get; set; }

        public ArgumentValueUIArgs Clone()
        {
            var newInstance = new ArgumentValueUIArgs();
            newInstance.Parent = Parent;
            newInstance.InitialValue = InitialValue;
            newInstance.ContextText = ContextText;
            newInstance.Type = Type;
            newInstance.Required = Required;
            newInstance.CatalogueRepository = CatalogueRepository;
            newInstance.Setter = Setter;
            newInstance.Fatal = Fatal;
            
            return newInstance;
        }
    }
}
