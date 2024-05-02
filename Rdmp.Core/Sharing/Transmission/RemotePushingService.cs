// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Rdmp.Core.Curation.Data.ImportExport;
using Rdmp.Core.Curation.Data.Remoting;
using Rdmp.Core.Curation.Data.Serialization;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Progress;
using Rdmp.Core.Sharing.Dependency.Gathering;

namespace Rdmp.Core.Sharing.Transmission;

/// <summary>
///     Serializes collections of RDMP objects into BINARY Json and streams to a RemoteRDMP endpoint.
/// </summary>
public class RemotePushingService
{
    private readonly IRDMPPlatformRepositoryServiceLocator _repositoryLocator;
    private readonly IDataLoadEventListener listener;
    private readonly IEnumerable<RemoteRDMP> remotes;
    private readonly Gatherer _gatherer;
    private readonly ShareManager _shareManager;

    public RemotePushingService(IRDMPPlatformRepositoryServiceLocator repositoryLocator,
        IDataLoadEventListener listener)
    {
        _repositoryLocator = repositoryLocator;
        this.listener = listener;
        remotes = _repositoryLocator.CatalogueRepository.GetAllObjects<RemoteRDMP>();
        _gatherer = new Gatherer(_repositoryLocator);
        _shareManager = new ShareManager(_repositoryLocator);
    }

    public async void SendToAllRemotes<T>(T[] toSendAll, Action callback = null) where T : IMapsDirectlyToDatabaseTable
    {
        listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
            $"Ready to send {toSendAll.Length} {typeof(T).Name} items to all remotes."));
        var done = new Dictionary<string, int>();

        foreach (var remoteRDMP in remotes)
            listener.OnProgress(this,
                new ProgressEventArgs(remoteRDMP.Name,
                    new ProgressMeasurement(0, ProgressType.Records, toSendAll.Length), new TimeSpan()));

        var tasks = new List<Task>();

        foreach (var remote in remotes)
        {
            done.Add(remote.Name, 0);

            foreach (var toSend in toSendAll)
            {
                if (!_gatherer.CanGatherDependencies(toSend))
                    throw new Exception(
                        $"Type {typeof(T)} is not supported yet by Gatherer and therefore cannot be shared");

                var share = _gatherer.GatherDependencies(toSend).ToShareDefinitionWithChildren(_shareManager);
                var json = JsonConvertExtensions.SerializeObject(share, _repositoryLocator);

                var handler = new HttpClientHandler
                {
                    Credentials = new NetworkCredential(remote.Username, remote.GetDecryptedPassword())
                };

                HttpResponseMessage result;

                var apiUrl = remote.GetUrlFor<T>();

                var remote1 = remote;
                var toSend1 = toSend;

                var sender = new Task(() =>
                {
                    using var client = new HttpClient(handler);
                    try
                    {
                        result = client.PostAsync(new Uri(apiUrl), new StringContent(json, Encoding.UTF8, "text/plain"))
                            .Result;
                        if (result.IsSuccessStatusCode)
                            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
                                $"Sending {toSend1} to {remote1.Name} completed."));
                        else
                            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error,
                                $"Error sending {toSend1} to {remote1.Name}: {result.ReasonPhrase} - {result.Content.ReadAsStringAsync().Result}"));
                        lock (done)
                        {
                            listener.OnProgress(this,
                                new ProgressEventArgs(remote1.Name,
                                    new ProgressMeasurement(++done[remote1.Name], ProgressType.Records,
                                        toSendAll.Length), new TimeSpan()));
                        }
                    }
                    catch (Exception ex)
                    {
                        listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error,
                            $"Error sending {toSend1} to {remote1.Name}", ex));
                        listener.OnProgress(this,
                            new ProgressEventArgs(remote1.Name, new ProgressMeasurement(1, ProgressType.Records, 1),
                                new TimeSpan()));
                    }
                });
                sender.Start();
                tasks.Add(sender);
            }
        }

        await Task.WhenAll(tasks);

        callback?.Invoke();
    }
}