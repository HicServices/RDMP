using System.ComponentModel.Composition;

namespace DataExportLibrary.DataRelease
{
    [Export(typeof(ReleaseEngineSettingsFactory))]
    public class ReleaseEngineSettingsFactory
    {
        public ReleaseEngineSettings CreateSettings()
        {
            return new ReleaseEngineSettings();
        }
    }
}