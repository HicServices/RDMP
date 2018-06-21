using System;
using System.Drawing;
using DataExportLibrary.Interfaces.ExtractionTime.Commands;

namespace CatalogueManager.Icons.IconProvision.StateBasedIconProviders
{
    public class ExtractCommandStateBasedIconProvider : IObjectStateBasedIconProvider
    {
        private Bitmap _waiting;
        private Bitmap _warning;
        private Bitmap _writing;
        private Bitmap _failed;
        private Bitmap _tick;

        public ExtractCommandStateBasedIconProvider()
        {
            _waiting = CatalogueIcons.Waiting;
            _warning = CatalogueIcons.Warning;
            _writing = CatalogueIcons.Writing;
            _failed = CatalogueIcons.Failed;
            _tick = CatalogueIcons.Tick;
        }
        public Bitmap GetImageIfSupportedObject(object o)
        {
            if (!(o is ExtractCommandState))
                return null;

            var ecs = (ExtractCommandState) o;

            switch (ecs)
            {
                case ExtractCommandState.NotLaunched:
                    return _waiting;
                case ExtractCommandState.WaitingForSQLServer:
                    return _waiting;
                case ExtractCommandState.WritingToFile:
                    return _writing;
                case ExtractCommandState.Crashed:
                    return _failed;
                case ExtractCommandState.UserAborted:
                    return _failed;
                case ExtractCommandState.Completed:
                    return _tick;
                case ExtractCommandState.Warning:
                    return _warning;
                case ExtractCommandState.WritingMetadata:
                    return _writing;
                case ExtractCommandState.WaitingToExecute:
                    return _waiting;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}