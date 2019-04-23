// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using CatalogueLibrary.ExternalDatabaseServerPatching;
using MapsDirectlyToDatabaseTable.Versioning;

namespace RDMPStartup.Events
{
    /// <summary>
    /// EventArgs for finding Plugin IPatchers during Startup.cs
    /// 
    /// <para>IPatchers identify databases that are managed by a .Database assembly and as such need to be patched/updated when the host assembly is updated.  For 
    /// plugins this is done by declaring a IPluginPatcher and listing the host/database assemblies but there can be Type loading errors or other Exceptions 
    /// around locating databases that must be patched, this event system supports reporting those.</para>
    /// </summary>
    public class PluginPatcherFoundEventArgs
    {
        public PluginPatcherFoundEventArgs(Type type, IPatcher instance, PluginPatcherStatus status, Exception exception=null)
        {
            Type = type;
            Instance = instance;
            Status = status;
            Exception = exception;
        }

        public Type Type { get; set; }
        public IPatcher Instance { get; set; } 
        public PluginPatcherStatus Status {get;set;}
        public Exception Exception { get; set; }
    }
}
