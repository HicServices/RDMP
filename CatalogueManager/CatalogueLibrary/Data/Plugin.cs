using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.IO.Compression;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.Data
{
    /// <summary>
    /// A collection of LoadModuleAssembly objects that make up a complete Plugin.  The Plugin is the head in which a name, upload location and verison are recorded then each
    /// dll that makes up the functionality is linked as LoadModuleAssemblies (See LoadModuleAssembly)
    /// </summary>
    public class Plugin : DatabaseEntity
    {
        #region Database Properties

        private string _name;
        private string _uploadedFromDirectory;
        private Version _pluginVersion;

        public string Name
        {
            get { return _name; }
            set { SetField(ref  _name, value); }
        }

        public string UploadedFromDirectory
        {
            get { return _uploadedFromDirectory; }
            set { SetField(ref  _uploadedFromDirectory, value); }
        }

        public Version PluginVersion
        {
            get { return _pluginVersion; }
            set { SetField(ref  _pluginVersion, value); }
        }

        #endregion

        public Plugin(ICatalogueRepository repository, FileInfo pluginZipFile)
        {
            repository.InsertAndHydrate(this, new Dictionary<string, object>()
            {
                {"Name", pluginZipFile.Name},
                {"UploadedFromDirectory",pluginZipFile.DirectoryName}
            });
            
        }

        public Plugin(ICatalogueRepository repository, DbDataReader r) : base(repository, r)
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

        public override string ToString()
        {
            return Name;
        }

        #region Relationships
        [NoMappingToDatabase]
        public IEnumerable<LoadModuleAssembly> LoadModuleAssemblies { get
        {
            return Repository.GetAllObjectsWithParent<LoadModuleAssembly>(this);
        } }
        #endregion

        public string GetPluginDirectoryName(DirectoryInfo downloadDirectoryRoot)
        {
            return downloadDirectoryRoot.FullName +"\\"+ Name.Replace(".zip", "");
        }
    }
}