using System;
using System.Text;
using CatalogueLibrary.Data;

namespace CatalogueManager.LogViewer
{
    public class LogViewerFilterCollection
    {
        private int? _task;
        private int? _run;
        private int? _table;
        public event Action FilterChanged = delegate { };

        public int? Task
        {
            get { return _task; }
            set
            {
                int? before = _task;

                _task = value;

                //if it changed
                if (before != value)
                    FilterChanged();
            }
        }

        public int? Run
        {
            get { return _run; }
            set
            {
                int? before = _run;

                _run = value;

                //if it changed
                if(before != value)
                    FilterChanged();
            }
        }

        public int? Table
        {
            get { return _table; }
            set
            {
                int? before = _table;

                _table = value;

                if(before != value)
                    FilterChanged();
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("DataLoadTask=" + (_task == null ? "<Null>" : Task.ToString()));
            sb.Append(" DataLoadRun=" + (_run == null ? "<Null>" : Run.ToString()));
            sb.Append(" TableLoadRun=" + (_table == null ? "<Null>" : Table.ToString()));

            return sb.ToString();
        }

        public void Clear()
        {
            _task = null;
            _run = null;
            _table = null;
            FilterChanged();
        }
    }
}