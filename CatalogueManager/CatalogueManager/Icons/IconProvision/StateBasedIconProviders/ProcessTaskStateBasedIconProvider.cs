using System;
using System.Drawing;
using CatalogueLibrary.Data.DataLoad;

namespace CatalogueManager.Icons.IconProvision.StateBasedIconProviders
{
    public class ProcessTaskStateBasedIconProvider : IObjectStateBasedIconProvider
    {
        private Bitmap _exe;
        private Bitmap _sql;
        private Bitmap _plugin;

        public ProcessTaskStateBasedIconProvider()
        {
            _exe = CatalogueIcons.Exe;
            _sql = CatalogueIcons.SQL;
            _plugin = CatalogueIcons.Plugin;

        }

        public Bitmap GetImageIfSupportedObject(object o)
        {
            var pt = o as ProcessTask;

            if(o is Type && o.Equals(typeof(ProcessTask)))
                return _plugin;

            if (pt == null)
                return null;

            switch (pt.ProcessTaskType)
            {
                case ProcessTaskType.Executable:
                    return _exe;
                case ProcessTaskType.SQLFile:
                    return _sql;
                case ProcessTaskType.StoredProcedure:
                    return _sql;
                case ProcessTaskType.Attacher:
                    return _plugin;
                case ProcessTaskType.DataProvider:
                    return _plugin;
                case ProcessTaskType.MutilateDataTable:
                    return _plugin;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}