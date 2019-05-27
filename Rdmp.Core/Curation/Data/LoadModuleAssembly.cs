// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Attributes;
using MapsDirectlyToDatabaseTable.Injection;
using Rdmp.Core.Curation.Data.ImportExport;
using Rdmp.Core.Curation.Data.Serialization;
using Rdmp.Core.Repositories;

namespace Rdmp.Core.Curation.Data
{
    /// <summary>
    /// This entity is a DLL (Dynamic Link Library - AKA Assembly) of compiled C# code that is either a MEF (Managed Extensibility Framework) plugin or a dependency of a MEF
    /// plugin.  Plugins add third party extension functionality (not part of the core RDMP functionality).  You can commit your compiled dlls by packaging them with 
    /// package.bat (or by zipping up your bin directory files) and committing the .zip via PluginManagementForm (Accessible via Ctrl+R).  PluginManagementForm will upload
    /// the DLL as a binary and pushed into the LoadModuleAssembly table.  This allows everyone using your Catalogue database access to the [Exports] defined in the compiled dll.
    /// 
    /// <para>A typical use case for this is when you are required to load a particularly freaky data format (e.g. even records are in UTF8 binary and odd records are in ASCII) which
    /// requires specific code to execute.  You would make a class for dealing with the file format and make it implement IPluginAttacher.  Upload your dll along with any
    /// dependency dlls and the next time a DataAnalyst is building a load configuration your attacher will be displayed along with all the 'out of the box' attachers (CSV, Excel etc)</para>
    /// </summary>
    public class LoadModuleAssembly : DatabaseEntity, IInjectKnown<Plugin>
    {
        /// <summary>
        /// List of dlls which will not be packaged up if present in your plugins bin directory since they already form part of the RDMP core architecture
        /// </summary>
        public static readonly string[] IgnoredDlls = new []
        {
            //part of main Platform
"clrcompression.dll",
"clretwrc.dll",
"clrjit.dll",
"CommandLine.dll",
"coreclr.dll",
"CsvHelper.dll",
"Databases.yaml",
"dbgshim.dll",
"ExcelNumberFormat.dll",
"FAnsi.dll",
"FAnsi.Implementations.MicrosoftSQL.dll",
"FAnsi.Implementations.MySql.dll",
"FAnsi.Implementations.Oracle.dll",
"Google.Protobuf.dll",
"hostfxr.dll",
"hostpolicy.dll",
"ICSharpCode.SharpZipLib.dll",
"MapsDirectlyToDatabaseTable.dll",
"Microsoft.CSharp.dll",
"Microsoft.DiaSymReader.Native.amd64.dll",
"Microsoft.VisualBasic.dll",
"Microsoft.Win32.Primitives.dll",
"Microsoft.Win32.Registry.dll",
"Microsoft.Win32.SystemEvents.dll",
"mscordaccore.dll",
"mscordaccore_amd64_amd64_4.6.27521.02.dll",
"mscordbi.dll",
"mscorlib.dll",
"mscorrc.debug.dll",
"mscorrc.dll",
"MySql.Data.dll",
"netstandard.dll",
"Newtonsoft.Json.dll",
"NLog.config",
"NLog.dll",
"NPOI.dll",
"NPOI.OOXML.dll",
"NPOI.OpenXml4Net.dll",
"NPOI.OpenXmlFormats.dll",
"NuDoq.dll",
"Oracle.ManagedDataAccess.dll",
"Plugin.Settings.Abstractions.dll",
"Plugin.Settings.dll",
"Rdmp.Core.dll",
"rdmp.deps.json",
"rdmp.dll",
"rdmp.dll.config",
"rdmp.runtimeconfig.json",
"Renci.SshNet.dll",
"ReusableLibraryCode.dll",
"sni.dll",
"sos.dll",
"SOS.NETCore.dll",
"sos_amd64_amd64_4.6.27521.02.dll",
"SshNet.Security.Cryptography.dll",
"System.AppContext.dll",
"System.Buffers.dll",
"System.Collections.Concurrent.dll",
"System.Collections.dll",
"System.Collections.Immutable.dll",
"System.Collections.NonGeneric.dll",
"System.Collections.Specialized.dll",
"System.ComponentModel.Annotations.dll",
"System.Composition.dll",
"System.ComponentModel.DataAnnotations.dll",
"System.ComponentModel.dll",
"System.ComponentModel.EventBasedAsync.dll",
"System.ComponentModel.Primitives.dll",
"System.ComponentModel.TypeConverter.dll",
"System.Configuration.ConfigurationManager.dll",
"System.Configuration.dll",
"System.Console.dll",
"System.Core.dll",
"System.Data.Common.dll",
"System.Data.dll",
"System.Data.SqlClient.dll",
"System.Diagnostics.Contracts.dll",
"System.Diagnostics.Debug.dll",
"System.Diagnostics.DiagnosticSource.dll",
"System.Diagnostics.FileVersionInfo.dll",
"System.Diagnostics.Process.dll",
"System.Diagnostics.StackTrace.dll",
"System.Diagnostics.TextWriterTraceListener.dll",
"System.Diagnostics.Tools.dll",
"System.Diagnostics.TraceSource.dll",
"System.Diagnostics.Tracing.dll",
"System.dll",
"System.Drawing.Common.dll",
"System.Drawing.dll",
"System.Drawing.Primitives.dll",
"System.Dynamic.Runtime.dll",
"System.Globalization.Calendars.dll",
"System.Globalization.dll",
"System.Globalization.Extensions.dll",
"System.IO.Compression.Brotli.dll",
"System.IO.Compression.dll",
"System.IO.Compression.FileSystem.dll",
"System.IO.Compression.ZipFile.dll",
"System.IO.dll",
"System.IO.FileSystem.AccessControl.dll",
"System.IO.FileSystem.dll",
"System.IO.FileSystem.DriveInfo.dll",
"System.IO.FileSystem.Primitives.dll",
"System.IO.FileSystem.Watcher.dll",
"System.IO.IsolatedStorage.dll",
"System.IO.MemoryMappedFiles.dll",
"System.IO.Pipes.AccessControl.dll",
"System.IO.Pipes.dll",
"System.IO.UnmanagedMemoryStream.dll",
"System.Linq.dll",
"System.Linq.Expressions.dll",
"System.Linq.Parallel.dll",
"System.Linq.Queryable.dll",
"System.Memory.dll",
"System.Net.dll",
"System.Net.Http.dll",
"System.Net.HttpListener.dll",
"System.Net.Mail.dll",
"System.Net.NameResolution.dll",
"System.Net.NetworkInformation.dll",
"System.Net.Ping.dll",
"System.Net.Primitives.dll",
"System.Net.Requests.dll",
"System.Net.Security.dll",
"System.Net.ServicePoint.dll",
"System.Net.Sockets.dll",
"System.Net.WebClient.dll",
"System.Net.WebHeaderCollection.dll",
"System.Net.WebProxy.dll",
"System.Net.WebSockets.Client.dll",
"System.Net.WebSockets.dll",
"System.Numerics.dll",
"System.Numerics.Vectors.dll",
"System.ObjectModel.dll",
"System.Private.CoreLib.dll",
"System.Private.DataContractSerialization.dll",
"System.Private.Uri.dll",
"System.Private.Xml.dll",
"System.Private.Xml.Linq.dll",
"System.Reflection.DispatchProxy.dll",
"System.Reflection.dll",
"System.Reflection.Emit.dll",
"System.Reflection.Emit.ILGeneration.dll",
"System.Reflection.Emit.Lightweight.dll",
"System.Reflection.Extensions.dll",
"System.Reflection.Metadata.dll",
"System.Reflection.Primitives.dll",
"System.Reflection.TypeExtensions.dll",
"System.Resources.Reader.dll",
"System.Resources.ResourceManager.dll",
"System.Resources.Writer.dll",
"System.Runtime.CompilerServices.Unsafe.dll",
"System.Runtime.CompilerServices.VisualC.dll",
"System.Runtime.dll",
"System.Runtime.Extensions.dll",
"System.Runtime.Handles.dll",
"System.Runtime.InteropServices.dll",
"System.Runtime.InteropServices.RuntimeInformation.dll",
"System.Runtime.InteropServices.WindowsRuntime.dll",
"System.Runtime.Loader.dll",
"System.Runtime.Numerics.dll",
"System.Runtime.Serialization.dll",
"System.Runtime.Serialization.Formatters.dll",
"System.Runtime.Serialization.Json.dll",
"System.Runtime.Serialization.Primitives.dll",
"System.Runtime.Serialization.Xml.dll",
"System.Security.AccessControl.dll",
"System.Security.Claims.dll",
"System.Security.Cryptography.Algorithms.dll",
"System.Security.Cryptography.Cng.dll",
"System.Security.Cryptography.Csp.dll",
"System.Security.Cryptography.Encoding.dll",
"System.Security.Cryptography.OpenSsl.dll",
"System.Security.Cryptography.Primitives.dll",
"System.Security.Cryptography.ProtectedData.dll",
"System.Security.Cryptography.X509Certificates.dll",
"System.Security.dll",
"System.Security.Permissions.dll",
"System.Security.Principal.dll",
"System.Security.Principal.Windows.dll",
"System.Security.SecureString.dll",
"System.ServiceModel.Web.dll",
"System.ServiceProcess.dll",
"System.Text.Encoding.CodePages.dll",
"System.Text.Encoding.dll",
"System.Text.Encoding.Extensions.dll",
"System.Text.RegularExpressions.dll",
"System.Threading.dll",
"System.Threading.Overlapped.dll",
"System.Threading.Tasks.Dataflow.dll",
"System.Threading.Tasks.dll",
"System.Threading.Tasks.Extensions.dll",
"System.Threading.Tasks.Parallel.dll",
"System.Threading.Thread.dll",
"System.Threading.ThreadPool.dll",
"System.Threading.Timer.dll",
"System.Transactions.dll",
"System.Transactions.Local.dll",
"System.ValueTuple.dll",
"System.Web.dll",
"System.Web.HttpUtility.dll",
"System.Windows.dll",
"System.Xml.dll",
"System.Xml.Linq.dll",
"System.Xml.ReaderWriter.dll",
"System.Xml.Serialization.dll",
"System.Xml.XDocument.dll",
"System.Xml.XmlDocument.dll",
"System.Xml.XmlSerializer.dll",
"System.Xml.XPath.dll",
"System.Xml.XPath.XDocument.dll",
"System.Xml.XPath.XmlDocument.dll",
"ucrtbase.dll",
"WindowsBase.dll",
"YamlDotNet.dll",
"AutocompleteMenu-ScintillaNET.dll",
"BadMedicine.Core.dll",
"DeltaCompressionDotNet.dll",
"DeltaCompressionDotNet.MsDelta.dll",
"DeltaCompressionDotNet.PatchApi.dll",
"en_US.aff",
"en_US.dic",
"Hunspellx64.dll",
"Hunspellx86.dll",
"hyph_en_US.dic",
"MathNet.Numerics.dll",
"Mono.Cecil.dll",
"Mono.Cecil.Mdb.dll",
"Mono.Cecil.Pdb.dll",
"Mono.Cecil.Rocks.dll",
"NHunspell.dll",
"NuGet.Squirrel.dll",
"ObjectListView.dll",
"Rdmp.UI.dll",
"ResearchDataManagementPlatform.application",
"ResearchDataManagementPlatform.exe.config",
"ResearchDataManagementPlatform.exe.manifest",
"ReusableUIComponents.dll",
"ScintillaNET.dll",
"SharpCompress.dll",
"Splat.dll",
"Squirrel.dll",
"WeifenLuo.WinFormsUI.Docking.dll",
"WeifenLuo.WinFormsUI.Docking.ThemeVS2015.dll"
        };

       #region Database Properties
        private string _name;
        private string _description;
        private Byte[] _dll;
        private Byte[] _pdb;
        private string _committer;
        private DateTime _uploadDate;
        private string _dllFileVersion;
        private int _plugin_ID;
        private Lazy<Plugin> _knownPlugin;

        /// <summary>
        /// The name of the dll or src file within the <see cref="Plugin"/>
        /// </summary>
        public string Name
        {
	        get { return _name;}
	        set { SetField(ref _name,value);}
        }

        /// <summary>
        /// Not currently used
        /// </summary>
        public string Description
        {
	        get { return _description;}
	        set { SetField(ref _description,value);}
        }

        /// <summary>
        /// The assembly (dll) file as a Byte[], use File.WriteAllBytes to write it to disk
        /// </summary>
        public Byte[] Dll
        {
	        get { return _dll;}
	        set { SetField(ref _dll,value);}
        }

        /// <summary>
        /// The assembly (pdb) file if any for the <see cref="Dll"/> which contains debugging symbols
        /// as a Byte[], use File.WriteAllBytes to write it to disk
        /// </summary>
        public Byte[] Pdb
        {
	        get { return _pdb;}
	        set { SetField(ref _pdb,value);}
        }

        /// <summary>
        /// The user who uploaded the dll
        /// </summary>
        public string Committer
        {
	        get { return _committer;}
	        set { SetField(ref _committer,value);}
        }

        /// <summary>
        /// The date the dll was uploaded
        /// </summary>
        public DateTime UploadDate
        {
	        get { return _uploadDate;}
	        set { SetField(ref _uploadDate,value);}
        }

        /// <summary>
        /// The version number of the dll
        /// </summary>
        public string DllFileVersion
        {
	        get { return _dllFileVersion;}
	        set { SetField(ref _dllFileVersion,value);}
        }

        /// <summary>
        /// The plugin this file forms a part of (each <see cref="Plugin"/> will usually have multiple dlls as part of it's dependencies)
        /// </summary>
        [Relationship(typeof(Plugin), RelationshipType.SharedObject)]
        public int Plugin_ID
        {
	        get { return _plugin_ID;}
	        set { SetField(ref _plugin_ID,value);}
        }

        #endregion

        #region Relationships
        
        /// <inheritdoc cref="Plugin_ID"/>
        [NoMappingToDatabase]
        public Plugin Plugin { get { return _knownPlugin.Value; }}

        #endregion

        /// <summary>
        /// Uploads the given dll file to the catalogue database ready for use as a plugin within RDMP (also uploads any pdb file in the same dir)
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="f"></param>
        public LoadModuleAssembly(ICatalogueRepository repository, FileInfo f, Plugin plugin)
        {
            var dictionaryParameters = GetDictionaryParameters(f, plugin);

            //so we can reference it in fetch requests to check for duplication (normaly Repository is set during hydration by the repo)
            Repository = repository;

            Repository.InsertAndHydrate(this,dictionaryParameters);
            ClearAllInjections();
        }

        internal LoadModuleAssembly(ICatalogueRepository repository, DbDataReader r)
            : base(repository, r)
        {
            Dll = r["Dll"] as byte[];
            Pdb = r["Pdb"] as byte[];
            Name = (string) r["Name"];
            Description = r["Description"] as string;
            Committer = r["Committer"] as string;
            UploadDate = Convert.ToDateTime(r["UploadDate"]);
            DllFileVersion = r["DllFileVersion"] as string;
            Plugin_ID = Convert.ToInt32(r["Plugin_ID"]);
            ClearAllInjections();
        }
        
        internal LoadModuleAssembly(ShareManager shareManager, ShareDefinition shareDefinition)
        {
            shareManager.UpsertAndHydrate(this, shareDefinition);
            ClearAllInjections();
        }

        /// <summary>
        /// Returns true if the file is on the list of <see cref="ProhibitedDllNames"/>
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public static bool IsDllProhibited(FileInfo f)
        {
            return IgnoredDlls.Contains(f.Name);
        }
        
        /// <summary>
        /// Downloads the plugin dll/pdb/src to the given directory
        /// </summary>
        /// <param name="downloadDirectory"></param>
        public void DownloadAssembly(DirectoryInfo downloadDirectory)
        {
            string targetDirectory = downloadDirectory.FullName;

            if (targetDirectory == null)
                throw new Exception("Could not get currently executing assembly directory");

            if (!downloadDirectory.Exists)
                downloadDirectory.Create();

            string targetFile = Path.Combine(targetDirectory, Name);
            
            //file already exists
            if (File.Exists(targetFile))
                if(AreEqual(File.ReadAllBytes(targetFile), Dll))
                    return;

            int timeout = 5000;

            TryAgain:
            try
            {
                //if it has changed length or does not exist, write it out to the hardisk
                File.WriteAllBytes(targetFile, Dll);
                
                if (Pdb != null)
                {
                    string pdbFilename = Path.Combine(targetDirectory,
                        Name.Substring(0, Name.Length - ".dll".Length) + ".pdb");
                    File.WriteAllBytes(pdbFilename, Pdb);
                }
            }
            catch (Exception)
            {
                timeout -= 100;
                Thread.Sleep(100);

                if (timeout <= 0)
                    throw;

                goto TryAgain;
            }
        }

        private Dictionary<string, object> GetDictionaryParameters(FileInfo f, Plugin plugin)
        {
            byte[] allPdbBytes = null;
            string version = null;

            //always allowed
            if (f.Name != "src.zip")
            {
                if (!f.Extension.ToLower().Equals(".dll"))
                    throw new NotSupportedException("Only .dll files can be commited");

                if (IgnoredDlls.Contains(f.Name))
                    throw new ArgumentException("Cannot commit assembly " + f.Name + " because it is a prohibited dll or has the word 'Test' in its filename");

                var pdb = new FileInfo(f.FullName.Substring(0, f.FullName.Length - ".dll".Length) + ".pdb");
                if (pdb.Exists)
                    allPdbBytes = File.ReadAllBytes(pdb.FullName);

                try
                {
                    version = FileVersionInfo.GetVersionInfo(f.FullName).FileVersion;
                }
                catch (Exception)
                {
                    // couldn't get file version, nevermind maybe it is some kind of freaky dll type
                }
            }
            else
            {
                //source code
                version = "1.0";
            }


            string name = f.Name;
            byte[] allBytes = File.ReadAllBytes(f.FullName);

            var dictionaryParameters = new Dictionary<string, object>()
                {
                    {"Name",name},
                    {"Dll",allBytes},
                    {"DllFileVersion",version},
                    {"Committer",Environment.UserName},
                    {"Plugin_ID",plugin.ID}
                };

            if (allPdbBytes != null)
                dictionaryParameters.Add("Pdb", allPdbBytes);

            return dictionaryParameters;
        }

        /// <summary>
        /// Updates the current state to match the dll file on disk
        /// </summary>
        /// <param name="toCommit"></param>
        public void UpdateTo(FileInfo toCommit)
        {
            var dict = GetDictionaryParameters(toCommit, Plugin);
            Dll = (byte[])dict["Dll"];
            DllFileVersion = (string) dict["DllFileVersion"];
            Committer = (string) dict["Committer"];
            Pdb = dict.ContainsKey("Pdb") ? (byte[]) dict["Pdb"] : null;

            SaveToDatabase();
        }
        private bool AreEqual(byte[] readAllBytes, byte[] dll)
        {
            if (readAllBytes.Length != dll.Length)
                return false;

            for (int i = 0; i < dll.Length; i++)
                if (!readAllBytes[i].Equals(dll[i]))
                    return false;

            return true;
        }

        public void InjectKnown(Plugin instance)
        {
            _knownPlugin = new Lazy<Plugin>(() => instance);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Name;
        }

        public void ClearAllInjections()
        {
            _knownPlugin = new Lazy<Plugin>(() => Repository.GetObjectByID<Plugin>(Plugin_ID));
        }
    }
}
