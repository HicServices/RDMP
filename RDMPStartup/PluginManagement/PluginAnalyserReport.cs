// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Reflection;

namespace RDMPStartup.PluginManagement
{
    /// <summary>
    /// Describes all the MEF exports in the current dll (LoadModuleAssembly) which is part of a Plugin.  Each MEF exposed Type has a PluginPart created for it
    /// which documents the methods / API compatibility issues (See PluginPart).
    /// 
    /// <para>Overall the dll is given a PluginAssemblyStatus indicating whether it was loadable and whether the Types declared resolved correctly and were loaded into
    /// the AppDomain.</para>
    /// 
    /// <para>If a given dll is unloadable it won't have any PluginParts but should have a BadAssemblyException</para>
    /// 
    /// <para>All dlls are evaluated even when they are bundled with the plugin as dependencies of other dlls and do not contain any MEF classes themselves.  For example
    /// if you have a plugin for managing DICOM images you might have a third party dll for interacting with DICOM files as part of the Plugin.  This would also 
    /// have a PluginAnalyserReport which should indicate Health status and no PluginParts (assuming it has not MEF exports).</para>
    /// </summary>
    public class PluginAnalyserReport
    {
        public PluginAssemblyStatus Status { get; set; }
        public Assembly Assembly { get; set; }
        public Exception BadAssemblyException { get; set; }

        public List<PluginPart> Parts = new List<PluginPart>();
        
        public PluginAnalyserReport()
        {
            Parts = new List<PluginPart>();
        }
    }
}
