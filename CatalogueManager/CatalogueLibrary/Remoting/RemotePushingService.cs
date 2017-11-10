using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Automation;
using CatalogueLibrary.Data.Remoting;
using CatalogueLibrary.Repositories;
using Newtonsoft.Json;
using ReusableLibraryCode.Progress;
using ReusableLibraryCode.Serialization;

namespace CatalogueLibrary.Remoting
{
    public class RemotePushingService
    {
        private readonly IDataLoadEventListener listener;
        private readonly IEnumerable<RemoteRDMP> remotes;

        public RemotePushingService(ICatalogueRepository repository, IDataLoadEventListener listener)
        {
            this.listener = listener;
            remotes = repository.GetAllObjects<RemoteRDMP>();
        }

        public async void SendCollectionToAll<T>(T[] collection, Action callback = null)
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
    }
}