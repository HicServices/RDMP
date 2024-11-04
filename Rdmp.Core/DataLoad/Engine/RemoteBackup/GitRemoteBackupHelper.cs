using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data.DataLoad;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Rdmp.Core.DataLoad.Engine.RemoteBackup;

public class GitRemoteBackupHelper
{
    private IBasicActivateItems _activator;
    private LoadMetadata _loadMetadata;

    private string repoName = "testRepo"; //should be in the lmd
    private string repoLocation = "localhost:2222/git-server/repos/testRepo.git";
    //private string user = "git";
    public GitRemoteBackupHelper(IBasicActivateItems activator, LoadMetadata loadMetadata)
    {
        _activator = activator;
        _loadMetadata = loadMetadata;
        var cacheLocation = _loadMetadata.LocationOfCacheDirectory;
        if (!Directory.Exists($"{cacheLocation}/{repoName}"))
        {
            Directory.CreateDirectory($"{cacheLocation}/{repoName}");
        }
    }

    public void SaveLoadMetaDataToRemote()
    {

    }


    public void PullChanges()
    {
        var repoPath = Repository.Init($"{_loadMetadata.LocationOfCacheDirectory}/{repoName}");
        using (var repo = new Repository(repoPath))
        {
            var pullOptions = new PullOptions {};
            Commands.Pull(repo, new Signature("git", "todo@todo.com", DateTimeOffset.Now), pullOptions);
        }
    }

    private void CommitChanges()
    {
        var repoPath = Repository.Init($"{_loadMetadata.LocationOfCacheDirectory}/{repoName}");
        using(var repo = new Repository(repoPath)){
            Commands.Stage(repo, "*");
            var author = new Signature("git", "todo@todo.com", DateTimeOffset.Now);
            var committer = author;
            var commit = repo.Commit("hello world", author, committer);
            var remote = repo.Network.Remotes.Add("origin", repoLocation);
            var pushOptions = new PushOptions
            {
                //CredentialsProvider = new CredentialsHandler(
                //        (url, usernameFromUrl, types) =>
                //            new UsernamePasswordCredentials()
                //            {
                //                Username = userUsername,
                //                Password = userPassword,
                //            }
                //    ),
            };
            repo.Network.Push(remote, @"refs/heads/master", pushOptions);
        }
    }


    private void ConvertLoadMetaDataToFlatFiles()
    {
        var cacheLocation = _loadMetadata.LocationOfCacheDirectory;
        //lots of repatition here
        var linkedCatalogues = $"{cacheLocation}/{repoName}/linked_catalogues";
        if (!Directory.Exists(linkedCatalogues))
        {
            Directory.CreateDirectory(linkedCatalogues);
        }
        foreach (var catalogue in _loadMetadata.GetAllCatalogues())
        {
            var jsonString = JsonSerializer.Serialize(catalogue);
            File.WriteAllText($"{linkedCatalogues}/{catalogue.ID}.json", jsonString);
        }
        var processTasksLocation = $"{cacheLocation}/{repoName}/process_tasks";
        if (!Directory.Exists(processTasksLocation))
        {
            Directory.CreateDirectory(processTasksLocation);
        }
        var processTasks = _activator.RepositoryLocator.CatalogueRepository.GetAllObjectsWhere<ProcessTask>("LoadMetadata_ID", _loadMetadata.ID);
        foreach (var processTask in processTasks)
        {
            var jsonString = JsonSerializer.Serialize(processTask);
            File.WriteAllText($"{processTasksLocation}/{processTask.ID}.json", jsonString);
        }
        var processTaskArgumentsLocation = $"{cacheLocation}/{repoName}/process_task_argumentss";
        if (!Directory.Exists(processTaskArgumentsLocation))
        {
            Directory.CreateDirectory(processTaskArgumentsLocation);
        }
        var processTaskIds = processTasks.Select(pt => pt.ID);
        foreach (var processTaskArgument in _activator.RepositoryLocator.CatalogueRepository.GetAllObjects<ProcessTaskArgument>().Where(pta => processTaskIds.Contains(pta.ProcessTask_ID)))
        {
            var jsonString = JsonSerializer.Serialize(processTaskArgument);
            File.WriteAllText($"{processTaskArgumentsLocation}/{processTaskArgument.ID}.json", jsonString);
        }
    }
}
