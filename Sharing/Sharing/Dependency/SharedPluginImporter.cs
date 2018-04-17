using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.ImportExport;
using CatalogueLibrary.Repositories;
using ReusableLibraryCode.Checks;

namespace Sharing.Dependency
{
    /*
    /// <summary>
    /// Facilitiates importing plugins from a remote contributor (as a MapsDirectlyToDatabaseTableStatelessDefinition) and creating the local copies of the Plugin dlls
    /// in the local CatalogueRepository database.
    /// </summary>
    public class SharedPluginImporter
    {
        private ShareDefinition<Plugin> _plugin;
        private List<ShareDefinition<LoadModuleAssembly>> _lma;

        public SharedPluginImporter(Plugin plugin)
        {
            _plugin = new ShareDefinition<Plugin>(plugin);
            _lma = plugin.LoadModuleAssemblies.Select(s => new ShareDefinition<LoadModuleAssembly>(s)).ToList();
        }

        public SharedPluginImporter(ShareDefinition<Plugin> plugin, ShareDefinition<LoadModuleAssembly>[] loadModuleAssemblies)
        {
            _plugin = plugin;
            _lma = loadModuleAssemblies.ToList();
        }

        public SharedPluginImporter(string plugin, string loadModuleAssemblies)
        {
            var received = Convert.FromBase64String(plugin);
            BinaryFormatter bf = new BinaryFormatter();

            using (MemoryStream ms = new MemoryStream(received))
            {
                _plugin = (ShareDefinition<Plugin>) bf.Deserialize(ms);
            }

            received = Convert.FromBase64String(loadModuleAssemblies);
            using (MemoryStream ms = new MemoryStream(received))
            {
                _lma = ((ShareDefinition<LoadModuleAssembly>[]) bf.Deserialize(ms)).ToList();
            }
        }

        public Plugin Import(IRDMPPlatformRepositoryServiceLocator targetRepositoryLocator, ICheckNotifier collisionsAndSuggestions)
        {
            var catalogueRepository = targetRepositoryLocator.CatalogueRepository;
            
            var collision = catalogueRepository.GetAllObjects<Plugin>().SingleOrDefault(p => p.Name.Equals(_plugin.Properties["Name"]));

            if (collision != null)
            {
                var fix = collisionsAndSuggestions.OnCheckPerformed(new CheckEventArgs("Update existing Plugin with Name '" + collision.Name + "'", CheckResult.Warning));

                if (fix)
                {
                    //it's an update so get the current LoadModuleAssembly of the remote (out of date) Plugin
                    foreach (LoadModuleAssembly lma in collision.LoadModuleAssemblies)
                    {
                        var match = _lma.SingleOrDefault(n => lma.Name.Equals(n.Properties["Name"]));
                        
                        //LoadModuleAssembly is in the new Plugin too so overwrite it
                        if (match != null)
                        {
                            match.Rehydrate(lma);
                            //lma.Dll = (byte[]) match.Properties["Dll"];
                            //lma.Pdb = (byte[]) match.Properties["Pdb"];
                            //lma.Committer = (string) match.Properties["Committer"];
                            //lma.Description = (string)match.Properties["Description"];
                            //lma.DllFileVersion = (string)match.Properties["DllFileVersion"];
                            //lma.UploadDate = (DateTime)match.Properties["UploadDate"];
                            lma.SaveToDatabase();

                            //record that the LoadModuleAssembly was dealt with 
                            _lma.Remove(match);
                        }
                        else
                        {
                            //no longer part of the plugin so remove it
                            lma.DeleteInDatabase();
                        }
                    }

                    //now send any new dlls that aren't yet in the destination
                    foreach (var newNotArrivedYet in _lma)
                    {
                        newNotArrivedYet.Properties["Plugin_ID"] = collision.ID;
                        newNotArrivedYet.ImportObject(targetRepositoryLocator);
                    }

                    //it was an update to an existing one so return the existing one
                    return collision;
                }
                
                //user chose not to overwrite
                _plugin.Properties["Name"] = _plugin.Properties["Name"] + "(Copy " +  Guid.NewGuid().ToString().Substring(0,5) + ")"; //append a unique guid
            }
            
            //import the plugin (now that it has a unique name or never collided in the first place)
            var newPlugin = (Plugin)_plugin.ImportObject(targetRepositoryLocator);
            var newPluginID = newPlugin.ID;

            foreach (var definition in _lma)
            {
                definition.Properties["Plugin_ID"] = newPluginID;
                definition.ImportObject(targetRepositoryLocator);
            }
            return newPlugin;
        }
    }*/
}
