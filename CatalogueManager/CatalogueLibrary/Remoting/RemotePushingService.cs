using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Automation;
using CatalogueLibrary.Data.Remoting;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable.ObjectSharing;
using Newtonsoft.Json;
using ReusableLibraryCode.Progress;
using ReusableLibraryCode.Serialization;

namespace CatalogueLibrary.Remoting
{
    /// <summary>
    /// Serializes collections of RDMP objects into BINARY Json and streams to a RemoteRDMP endpoint.
    /// </summary>
    public class RemotePushingService
    {
        private readonly ICatalogueRepository _repository;
        private readonly IDataLoadEventListener listener;
        private readonly IEnumerable<RemoteRDMP> remotes;

        public RemotePushingService(ICatalogueRepository repository, IDataLoadEventListener listener)
        {
            _repository = repository;
            this.listener = listener;
            remotes = repository.GetAllObjects<RemoteRDMP>();
        }

        public async void SendCollectionToAllRemotes<T>(T[] collection, Action callback = null)
        {
            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Ready to send " + collection.Length + " " + typeof(T).Name + " items to all remotes."));

            foreach (var remoteRDMP in remotes)
            {
                listener.OnProgress(this, new ProgressEventArgs(remoteRDMP.Name, new ProgressMeasurement(0, ProgressType.Records), new TimeSpan()));
            }

            var tasks = new List<Task>();

            foreach (var remote in remotes)
            {
                var ignoreRepoResolver = new IgnorableSerializerContractResolver();
                ignoreRepoResolver.Ignore(typeof(DatabaseEntity), new[] { "Repository" });

                var settings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = ignoreRepoResolver,
                };
                var json = JsonConvert.SerializeObject(collection, Formatting.None, settings);

                var handler = new HttpClientHandler()
                {
                    Credentials = new NetworkCredential(remote.Username, remote.Password)
                };

                HttpResponseMessage result;

                var apiUrl = remote.GetUrlFor<T>(isarray: true);

                RemoteRDMP remote1 = remote;
                var sender = new Task(() =>
                {
                    using (var client = new HttpClient(handler))
                    {
                        try
                        {
                            result = client.PostAsync(new Uri(apiUrl), new StringContent(json, Encoding.UTF8, "application/json")).Result;
                            if (result.IsSuccessStatusCode)
                                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Sending message to " + remote1.Name + " completed."));
                            else
                                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, "Error sending message to " + remote1.Name + ": " + result.ReasonPhrase));
                        }
                        catch (Exception ex)
                        {
                            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, "Error sending message to " + remote1.Name, ex));
                            listener.OnProgress(this, new ProgressEventArgs(remote1.Name, new ProgressMeasurement(1, ProgressType.Records, 1), new TimeSpan()));
                        }
                    }
                });
                sender.Start();
                tasks.Add(sender);
            }

            await Task.WhenAll(tasks);

            if (callback != null)
                callback();
        }

        public async void SendPluginsToAllRemotes(Plugin[] plugins, Action callback = null)
        {
            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Ready to send " + plugins.Length + " " + typeof(Plugin).Name + " items to all remotes."));
            var done = new Dictionary<string, int>();

            foreach (var remoteRDMP in remotes)
            {
                listener.OnProgress(this, new ProgressEventArgs(remoteRDMP.Name, new ProgressMeasurement(0, ProgressType.Records, plugins.Length), new TimeSpan()));
            }

            var tasks = new List<Task>();

            foreach (var remote in remotes)
            {
                done.Add(remote.Name, 0);
                    
                foreach (var plugin in plugins)
                {
                    var pStateless = new MapsDirectlyToDatabaseTableStatelessDefinition<Plugin>(plugin);
                    var lmaStatelessArray =
                        plugin.LoadModuleAssemblies.Select(
                            lma => new MapsDirectlyToDatabaseTableStatelessDefinition<LoadModuleAssembly>(lma)).ToArray();

                    var bf = new BinaryFormatter();
                    string pluginString;
                    string lmasString;

                    using (var ms = new MemoryStream())
                    {
                        bf.Serialize(ms, pStateless);
                        pluginString = Convert.ToBase64String(ms.ToArray());
                    }

                    using (var ms = new MemoryStream())
                    {
                        bf.Serialize(ms, lmaStatelessArray);
                        lmasString = Convert.ToBase64String(ms.ToArray());
                    }

                    var pluginJson = JsonConvert.SerializeObject(new { pluginParam = pluginString, lmasParam = lmasString }, Formatting.None);

                    var handler = new HttpClientHandler()
                    {
                        Credentials = new NetworkCredential(remote.Username, remote.GetDecryptedPassword())
                    };

                    HttpResponseMessage result;

                    var apiUrl = remote.GetUrlFor<Plugin>();

                    RemoteRDMP remote1 = remote;
                    Plugin plugin1 = plugin;
                               
                    var sender = new Task(() =>
                    {
                        using (var client = new HttpClient(handler))
                        {
                            try
                            {
                                result = client.PostAsync(new Uri(apiUrl), new StringContent(pluginJson, Encoding.UTF8, "application/json")).Result;
                                if (result.IsSuccessStatusCode)
                                    listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Sending " + plugin1.Name + " to " + remote1.Name + " completed."));
                                else
                                    listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, "Error sending " + plugin1.Name + " to " + remote1.Name + ": " + result.ReasonPhrase));
                                lock (done)
                                {
                                    listener.OnProgress(this, new ProgressEventArgs(remote1.Name, new ProgressMeasurement(++done[remote1.Name], ProgressType.Records, plugins.Length), new TimeSpan()));   
                                }
                            }
                            catch (Exception ex)
                            {
                                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, "Error sending " + plugin1.Name + " to " + remote1.Name, ex));
                                listener.OnProgress(this, new ProgressEventArgs(remote1.Name, new ProgressMeasurement(1, ProgressType.Records, 1), new TimeSpan()));
                            }
                        }
                    });
                    sender.Start();
                    tasks.Add(sender);
                }
            }

            await Task.WhenAll(tasks);

            if (callback != null)
                callback();
        }
    }
}