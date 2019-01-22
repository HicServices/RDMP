using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.IO.Compression;
using CatalogueLibrary.Data.ImportExport;
using CatalogueLibrary.Data.Serialization;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.Data
{
    /// <summary>
    /// A collection of LoadModuleAssembly objects that make up a complete Plugin.  The Plugin is the head in which a name, upload location and verison are recorded then each
    /// dll that makes up the functionality is linked as LoadModuleAssemblies (See LoadModuleAssembly)
    /// </summary>
    public class Plugin : DatabaseEntity,INamed
    {
        #region Database Properties

        private string _name;
        private string _uploadedFromDirectory;
        private Version _pluginVersion;

        /// <inheritdoc/>
        public string Name
        {
            get { return _name; }
            set { SetField(ref  _name, value); }
        }

        /// <summary>
        /// Where the plugin files were uploaded from
        /// </summary>
        public string UploadedFromDirectory
        {
            get { return _uploadedFromDirectory; }
            set { SetField(ref  _uploadedFromDirectory, value); }
        }

        /// <summary>
        /// The master version of the <see cref="Plugin"/> (not the dlls inside - See <see cref="LoadModuleAssembly.DllFileVersion"/>).
        /// <para>Not currently used</para>
        /// </summary>
        public Version PluginVersion
        {
            get { return _pluginVersion; }
            set { SetField(ref  _pluginVersion, value); }
        }

        #endregion

        /// <summary>
        /// Defines a new collection of dlls that provide plugin functionality for RDMP
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="pluginZipFile"></param>
        public Plugin(ICatalogueRepository repository, FileInfo pluginZipFile, Version version = null)
        {
            repository.InsertAndHydrate(this, new Dictionary<string, object>()
            {
                {"Name", pluginZipFile.Name},
                {"UploadedFromDirectory", pluginZipFile.DirectoryName},
                {"PluginVersion", (version ?? new Version(0,0,0,0))}
            });
            
        }

        internal Plugin(ICatalogueRepository repository, DbDataReader r) : base(repository, r)
        {
            Name = r["Name"].ToString();
            UploadedFromDirectory = r["UploadedFromDirectory"].ToString();

            object o = r["PluginVersion"];

            if (o == DBNull.Value || o == null)
                PluginVersion = null;
            else
            {
                try
                {
                    PluginVersion = new Version(o.ToString());
                }
                catch (ArgumentException)
                {
                    PluginVersion = new Version("0.0.0.0");//user hacked database and typed in 'I've got a lovely bunch of coconuts' into the version field?
                }
            }
        }

        internal Plugin(ShareManager shareManager, ShareDefinition shareDefinition)
        {
            shareManager.RepositoryLocator.CatalogueRepository.UpsertAndHydrate(this, shareManager, shareDefinition);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Name;
        }

        #region Relationships

        /// <summary>
        /// Gets all the dlls and source code(if available) stored as <see cref="LoadModuleAssembly"/> in the catalogue database
        /// </summary>
        [NoMappingToDatabase]
        public IEnumerable<LoadModuleAssembly> LoadModuleAssemblies { get
        {
            return Repository.GetAllObjectsWithParent<LoadModuleAssembly>(this);
        } }
        #endregion

        /// <summary>
        /// Returns a folder name suitable for storing the dlls for the plugin in as a subdirectory of 
        /// <paramref name="downloadDirectoryRoot"/>
        /// </summary>
        /// <param name="downloadDirectoryRoot"></param>
        /// <returns></returns>
        public string GetPluginDirectoryName(DirectoryInfo downloadDirectoryRoot)
        {
            var pluginName = Path.GetFileNameWithoutExtension(Name);

            if(string.IsNullOrWhiteSpace(pluginName))
                throw new Exception("Plugin doens't have a valid name");

            return Path.Combine(downloadDirectoryRoot.FullName ,pluginName);
        }
    }
}