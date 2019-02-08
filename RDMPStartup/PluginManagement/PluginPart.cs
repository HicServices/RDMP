// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;

namespace RDMPStartup.PluginManagement
{
    /// <summary>
    /// Describes a Type declared in a dll (LoadModuleAssembly) that is part of a Plugin downloaded into the MEF directory and being analysed by a PluginAnalyser.
    /// Includes the MEF ComposablePartDefinition, Type and a list of all the decompiled methods on the class (PluginDependency). 
    /// </summary>
    public class PluginPart
    {
        public List<PluginDependency> Dependencies { get; private set; }
        
        public ComposablePartDefinition ExportPart { get; set; }
        public Type PartType;

        private PluginPart()
        {
            Dependencies = new List<PluginDependency>();
        }
        public PluginPart(ComposablePartDefinition part):this()
        {
            ExportPart = part;
        }

        public PluginPart(Type type): this()
        {
            PartType = type;
        }

        public override string ToString()
        {
            if (PartType != null)
                return PartType.FullName;
            
            return ExportPart.ToString();
        }
    }
}