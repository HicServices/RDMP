using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDMPStartup.Events
{
    public delegate void FoundPlatformDatabaseHandler(object sender, PlatformDatabaseFoundEventArgs eventArgs);
    public delegate void MEFDownloadProgressHandler(object sender, MEFFileDownloadProgressEventArgs eventArgs);

    public delegate void PluginPatcherFoundHandler(object sender, PluginPatcherFoundEventArgs eventArgs);
}
